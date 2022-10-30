using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameModeManagement;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;

public class ChoreographyReader : MonoBehaviour
{
    public static ChoreographyReader Instance { get; private set; }


    [SerializeField] private Choreography _choreography;

    private DifficultyInfo _difficultyInfo;

    private List<ChoreographyFormation> _sequenceables;
    private List<ChoreographyFormation> _formations;
    public ChoreographyNote[] Notes => _choreography.Notes;
    public ChoreographyEvent[] Events => _choreography.Events;
    public ChoreographyObstacle[] Obstacles => _choreography.Obstacles;

    public bool CanHaveBlock =>
        PlaylistManager.Instance.CurrentItem.TargetGameMode is not GameMode.JabsOnly or GameMode.OneHanded or GameMode.LightShow;
    public bool CanSwitchHands => PlaylistManager.Instance.CurrentItem.TargetGameMode is not GameMode.OneHanded;
    

    [Header("Settings")] public UnityEvent finishedLoadingSong = new UnityEvent();

    private CancellationTokenSource _cancellationSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        
        _sequenceables = new List<ChoreographyFormation>(5000);
        _formations = new List<ChoreographyFormation>(5000);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void LoadJson(PlaylistItem item)
    {
        AsyncLoadJson(item).Forget();
    }

    public void CancelLoad()
    {
        _cancellationSource?.Cancel();
    }

    private async UniTaskVoid AsyncLoadJson(PlaylistItem item)
    {
        var playlist = PlaylistManager.Instance.CurrentPlaylist;
        var gameMode = playlist.GameModeOverride == GameMode.Unset ? item.TargetGameMode : playlist.GameModeOverride;
        
        if (playlist.DifficultyEnum == DifficultyInfo.DifficultyEnum.Unset)
        {
            _difficultyInfo = item.SongInfo.TryGetActiveDifficultyInfo(item.DifficultyEnum, item.TargetGameMode);
        }
        else
        {
            _difficultyInfo = item.SongInfo.TryGetActiveDifficultyInfo(playlist.DifficultyEnum, gameMode);
        }
        
        if (_cancellationSource.IsCancellationRequested)
        {
            _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        }

        _choreography = await Choreography.AsyncLoadFromPlaylist(item, _difficultyInfo, _cancellationSource.Token);

        finishedLoadingSong?.Invoke();
    }

    public void ResetForNextSequence()
    {
        _formations?.Clear();
    }

    public List<ChoreographyFormation> GetOrderedFormations()
    {
        if (_formations == null || _formations.Count == 0)
        {
            GetChoreographSequenceables();
            
            UpdateFormation();
            _choreography = new Choreography();
        }

        return _formations;
    }

    private void GetChoreographSequenceables()
    {
        _sequenceables.Clear();

        var playlistGameMode = PlaylistManager.Instance.CurrentPlaylist.GameModeOverride;
        
        var targetGameMode = playlistGameMode == GameMode.Unset
            ? PlaylistManager.Instance.CurrentItem.TargetGameMode
            :playlistGameMode;

        var notes = Notes;
        var obstacles = Obstacles;
        switch (targetGameMode)
        {
            case GameMode.Unset:
                break;
            case GameMode.Normal:
                break;
            case GameMode.JabsOnly:
                notes = Choreography.SetNotesToType(notes, ChoreographyNote.CutDirection.Jab);
                break;
            case GameMode.OneHanded:
                notes = Choreography.SetNotesToSide(notes, HitSideType.Right);
                break;
            case GameMode.Degrees90:
                break;
            case GameMode.Degrees360:
                break;
            case GameMode.LegDay:
                break;
            case GameMode.NoObstacles:
                obstacles = null;
                break;
            case GameMode.LightShow:
                notes = null;
                obstacles = null;
                break;
            case GameMode.Lawless:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        for (var i = 0; i < (notes?.Length ?? 0); i++)
        {
            _sequenceables.Add(new ChoreographyFormation(notes[i]));
        }
        for (var i = 0; i < (obstacles?.Length??0); i++)
        {
            _sequenceables.Add(new ChoreographyFormation(obstacles[i]));
        }

        if (Events != null)
        {
            for (var i = 0; i < Events.Length; i++) //Need to process Events differently. TODO: Figure this out
            {
                switch (targetGameMode)
                {
                    case GameMode.Unset:
                    case GameMode.Normal:
                    case GameMode.JabsOnly:
                    case GameMode.OneHanded:
                    case GameMode.LegDay:
                    case GameMode.NoObstacles:
                        break;
                    case GameMode.LightShow:
                        break; //Will figure this out later
                    case GameMode.Degrees90:
                    case GameMode.Degrees360:
                    case GameMode.Lawless:
                        if (Events[i].Type is ChoreographyEvent.EventType.EarlyRotation
                            or ChoreographyEvent.EventType.LateRotation)
                        {
                            _sequenceables.Add(new ChoreographyFormation(Events[i]));
                        }

                        break;
                }
            }
        }

        _sequenceables.Sort((sequenceable0, sequenceable1) => sequenceable0.Time.CompareTo(sequenceable1.Time));
    }

    private void UpdateFormation()
    {
        if (_formations == null)
        {
            _formations = new List<ChoreographyFormation>(10000);
        }
        _formations.Clear();

        var lastTime = -1f;

        ChoreographyFormation lastSequence = new ChoreographyFormation();
        ChoreographyFormation thisSequence = new ChoreographyFormation();
        
        float lastRotation = -1f;
        var leftSidePriority = 0;
        var leftSideAdd = 0;
        var rightSidePriority = 0;
        var rightSideAdd = 0;
        var blockPriority = 0;
        var blockAdd = 0;

        var minTargetDistance = _difficultyInfo.MinTargetSpace;

        
        /*var tempSequenceables = new NativeArray<ChoreographyFormation>(_sequenceables.Count, Allocator.TempJob);
        var finalFormations = new NativeList<ChoreographyFormation>(_sequenceables.Count, Allocator.TempJob);
        for (var i = 0; i < _sequenceables.Count; i++)
        {
            tempSequenceables[i] = _sequenceables[i];
        }

        var data = new UpdateFormationsJob.Data(minTargetDistance, CanSwitchHands, CanHaveBlock, _difficultyInfo.DifficultyRank);
        var updateFormationsJob = new UpdateFormationsJob(data, tempSequenceables, finalFormations);
        
        var handle = new JobHandle();
        handle = updateFormationsJob.Schedule(tempSequenceables.Length, handle);
        handle.Complete();

        for (var i = 0; i < finalFormations.Length; i++)
        {
            _formations.Add(finalFormations[i]);
        }

        tempSequenceables.Dispose();
        finalFormations.Dispose();*/

        for (var i = 0; i < _sequenceables.Count; i++)
        {
            var sequenceable = _sequenceables[i];
            if (lastTime < sequenceable.Time)
            {
                if (!lastSequence.IsValid)
                {
                    lastTime = sequenceable.Time;
                }
                else
                {
                    var minGap = minTargetDistance;
                    if (sequenceable.HasNote)
                    {
                        var note = sequenceable.Note;
                        if (note.CutDir != ChoreographyNote.CutDirection.Jab &&
                            note.HitSideType != HitSideType.Block)
                        {
                            minGap *= 1.5f;
                        }
                    }
                    else if (sequenceable.HasObstacle)
                    {
                        minGap *= 2;
                    }
                    else if (sequenceable.HasEvent)
                    {
                        var chorEvent = sequenceable.Event;
                        if (chorEvent.Type is ChoreographyEvent.EventType.EarlyRotation
                            or ChoreographyEvent.EventType.LateRotation)
                        {
                            if (lastRotation + minGap * 5 < sequenceable.Time)
                            {
                                lastRotation = sequenceable.Time;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            //For now TODO: Lighting events.
                            continue;
                        }
                    }

                    if (sequenceable.HasNote || sequenceable.HasObstacle)
                    {
                        if (lastTime + minGap < sequenceable.Time)
                        {
                                lastTime = sequenceable.Time;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }

            if (Mathf.Abs(lastTime - sequenceable.Time) < .01f) //if lastTime == sequenceable.Time but for floats
            {
                if (sequenceable.HasNote)
                {
                    var note = sequenceable.Note;
                    var notePriority = note.HitSideType switch
                    {
                        HitSideType.Block => blockPriority,
                        HitSideType.Left => leftSidePriority,
                        HitSideType.Right => rightSidePriority,
                        _ => 0
                    };

                    var shouldSkipNote = thisSequence.HasNote;
                    if (shouldSkipNote)
                    {
                        var currentNote = thisSequence.Note;
                        var currentPriority = currentNote.HitSideType switch
                        {
                            HitSideType.Block => blockPriority,
                            HitSideType.Left => leftSidePriority,
                            HitSideType.Right => rightSidePriority,
                            _ => 0
                        };
                        shouldSkipNote = notePriority <= currentPriority;
                    }

                    if (!shouldSkipNote)
                    {
                        if (CanSwitchHands)
                        {
                            if (CanHaveBlock && notePriority < blockPriority &&
                                Mathf.Abs(notePriority - blockPriority) > 20)
                            {
                                note = note.SetToBlock();
                                blockPriority++;
                                if (blockAdd > 0)
                                {
                                    blockAdd--;
                                    if (blockAdd == 0)
                                    {
                                        blockPriority = notePriority;
                                    }
                                }
                                else
                                {
                                    blockAdd = 3;
                                }
                            }
                            else if (notePriority < leftSidePriority && Mathf.Abs(notePriority - leftSidePriority) > 10)
                            {
                                note = note.SetType(HitSideType.Left);
                                leftSidePriority++;
                                if (leftSideAdd > 0)
                                {
                                    leftSideAdd--;
                                    if (leftSideAdd == 0)
                                    {
                                        leftSidePriority = notePriority;
                                    }
                                }
                                else
                                {
                                    leftSideAdd = 3;
                                }
                            }
                            else if (notePriority < rightSidePriority &&
                                     Mathf.Abs(notePriority - rightSidePriority) > 10)
                            {
                                note = note.SetType(HitSideType.Right);
                                rightSidePriority++;
                                if (rightSideAdd > 0)
                                {
                                    rightSideAdd--;
                                    if (rightSideAdd == 0)
                                    {
                                        rightSidePriority = notePriority;
                                    }
                                }
                                else
                                {
                                    rightSideAdd = 3;
                                }
                            }
                        }

                        if (!thisSequence.HasObstacle)
                        {
                            note = note.SetCutDirection(ChoreographyNote.CutDirection.Jab);
                            note = note.SetLineLayer(ChoreographyNote.LineLayerType.Low);

                            switch (thisSequence.Obstacle.HitSideType)
                            {
                                case HitSideType.Block:
                                    break;
                                case HitSideType.Left:
                                    note = note.SetLineIndex(2);
                                    break;
                                case HitSideType.Right:
                                    note = note.SetLineIndex(1);
                                    break;
                                default:
                                    continue;
                            }
                        }

                        if (note.LineLayer == ChoreographyNote.LineLayerType.Low)
                        {
                            if (note.CutDir == ChoreographyNote.CutDirection.Uppercut)
                            {
                                note = note.SetToBasicJab();
                            }
                            else if (note.CutDir == ChoreographyNote.CutDirection.UppercutLeft ||
                                     note.CutDir == ChoreographyNote.CutDirection.UppercutRight)
                            {
                                note = note.SetToBlock();
                            }
                        }
                        else if (note.LineLayer == ChoreographyNote.LineLayerType.High)
                        {
                            if (note.CutDir == ChoreographyNote.CutDirection.JabDown ||
                                note.CutDir == ChoreographyNote.CutDirection.HookLeftDown ||
                                note.CutDir == ChoreographyNote.CutDirection.HookRightDown)
                            {
                                note = note.SetToBlock();
                            }
                        }

                        if (_difficultyInfo.DifficultyRank < 5)
                        {
                            if (note.LineLayer == ChoreographyNote.LineLayerType.High)
                            {
                                if (_difficultyInfo.DifficultyRank == 1 &&
                                    note.CutDir == ChoreographyNote.CutDirection.Jab)
                                {
                                    note = note.SetToBlock();
                                }
                                else if (note.CutDir == ChoreographyNote.CutDirection.JabDown ||
                                         note.CutDir == ChoreographyNote.CutDirection.HookLeftDown ||
                                         note.CutDir == ChoreographyNote.CutDirection.HookRightDown)
                                {
                                    note = note.SetToBasicJab();
                                }
                            }
                        }

                        switch (note.HitSideType)
                        {
                            case HitSideType.Block:
                                note = note.SetLineIndex(1);

                                blockPriority--;
                                break;
                            case HitSideType.Left:
                                if (note.LineIndex > 1 ||
                                    (note.CutDir == ChoreographyNote.CutDirection.Jab && note.LineIndex != 1))
                                {
                                    note = note.SetLineIndex(1);
                                }

                                if (note.CutDir == ChoreographyNote.CutDirection.HookLeft)
                                {
                                    note = note.SetToBasicJab();
                                }

                                leftSidePriority--;
                                break;
                            case HitSideType.Right:
                                if (note.LineIndex < 2 ||
                                    (note.CutDir == ChoreographyNote.CutDirection.Jab && note.LineIndex != 2))
                                {
                                    note = note.SetLineIndex(2);
                                }

                                if (note.CutDir == ChoreographyNote.CutDirection.HookRight)
                                {
                                    note = note.SetToBasicJab();
                                }

                                rightSidePriority--;
                                break;
                        }

                        thisSequence = thisSequence.SetNote(note);
                    }
                }
                else if (sequenceable.HasObstacle && !thisSequence.HasObstacle)
                {
                    var obstacle = thisSequence.Obstacle;
                    if (thisSequence.HasNote)
                    {
                        var tempNote = thisSequence.Note;
                        switch (tempNote.HitSideType)
                        {
                            case HitSideType.Block:
                                break;
                            case HitSideType.Left:
                                if (obstacle.LineIndex < 2)
                                {
                                    tempNote = tempNote.SetLineIndex(2);
                                }

                                break;
                            case HitSideType.Right:
                                if (obstacle.LineIndex > 1)
                                {
                                    tempNote = tempNote.SetLineIndex(1);
                                }

                                break;
                            default:
                                continue;
                        }

                        tempNote = tempNote.SetCutDirection(ChoreographyNote.CutDirection.Jab);
                        tempNote = tempNote.SetLineLayer(ChoreographyNote.LineLayerType.Low);
                        thisSequence = thisSequence.SetNote(tempNote);
                    }

                    thisSequence = thisSequence.SetObstacle(obstacle);
                }
                else if (sequenceable.HasEvent && !thisSequence.HasEvent)
                {
                    if (lastRotation + minTargetDistance * 5 < sequenceable.Time)
                    {
                        thisSequence = thisSequence.SetEvent(sequenceable.Event);
                        lastRotation = sequenceable.Time;
                    }
                }

                lastSequence = sequenceable;
            }
            else if (Mathf.Abs(lastRotation - sequenceable.Time) < .01f)
            {
                if (sequenceable.HasEvent && !thisSequence.HasEvent)
                {
                    thisSequence = thisSequence.SetEvent(sequenceable.Event);
                }
            }

            if ((i + 1 < _sequenceables.Count && _sequenceables[1 + i].Time > lastTime) || i + 1 == _sequenceables.Count)
            {
                _formations.Add(thisSequence);

                thisSequence = new ChoreographyFormation();
            }
        }
    }

    [BurstCompile]
    private struct UpdateFormationsJob : IJobFor
    {
        private Data _data;
        
        private readonly NativeArray<ChoreographyFormation> _sequenceables;

        private NativeList<ChoreographyFormation> _finalFormations;

        private ChoreographyFormation _lastSequence;
        private ChoreographyFormation _thisSequence;
        private float _lastTime;
        private float _lastRotation;
        private int _leftSidePriority;
        private int _leftSideAdd;
        private int _rightSidePriority;
        private int _rightSideAdd;
        private int _blockPriority;
        private int _blockAdd;
        
        public UpdateFormationsJob(Data data, NativeArray<ChoreographyFormation> formations, NativeList<ChoreographyFormation> finalFormations)
        {
            _data = data;
            
            _sequenceables = formations;
            _finalFormations = finalFormations;
            _lastTime = -1f;
            
            _thisSequence = new ChoreographyFormation();
            _lastSequence = new ChoreographyFormation();
        
            _lastRotation = -1f;
            _leftSidePriority = 0;
            _leftSideAdd = 0;
            _rightSidePriority = 0;
            _rightSideAdd = 0;
            _blockPriority = 0;
            _blockAdd = 0;
        }
        
        public void Execute(int index)
        {
            var formation = _sequenceables[index];
            if (_lastTime < formation.Time)
            {
                if (!_lastSequence.IsValid)
                {
                    _lastTime = formation.Time;
                }
                else
                {
                    var minGap = _data.MinTargetDistance;
                    if (formation.HasNote)
                    {
                        var note = formation.Note;
                        if (note.CutDir != ChoreographyNote.CutDirection.Jab &&
                            note.HitSideType != HitSideType.Block)
                        {
                            minGap *= 1.5f;
                        }
                    }
                    else if (formation.HasObstacle)
                    {
                        minGap *= 2;
                    }
                    else if (formation.HasEvent)
                    {
                        var chorEvent = formation.Event;
                        if (chorEvent.Type is ChoreographyEvent.EventType.EarlyRotation
                            or ChoreographyEvent.EventType.LateRotation)
                        {
                            if (_lastRotation + minGap * 5 < formation.Time)
                            {
                                _lastRotation = formation.Time;
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            //For now TODO: Lighting events.
                            return;
                        }
                    }

                    if (formation.HasNote || formation.HasObstacle)
                    {
                        if (_lastTime + minGap < formation.Time)
                        {
                                _lastTime = formation.Time;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }

            if (Mathf.Abs(_lastTime - formation.Time) < .01f) //if lastTime == sequenceable.Time but for floats
            {
                if (formation.HasNote)
                {
                    var note = formation.Note;
                    var notePriority = note.HitSideType switch
                    {
                        HitSideType.Block => _blockPriority,
                        HitSideType.Left => _leftSidePriority,
                        HitSideType.Right => _rightSidePriority,
                        _ => 0
                    };

                    var shouldSkipNote = _thisSequence.HasNote;
                    if (shouldSkipNote)
                    {
                        var currentNote = _thisSequence.Note;
                        var currentPriority = currentNote.HitSideType switch
                        {
                            HitSideType.Block => _blockPriority,
                            HitSideType.Left => _leftSidePriority,
                            HitSideType.Right => _rightSidePriority,
                            _ => 0
                        };
                        shouldSkipNote = notePriority <= currentPriority;
                    }

                    if (!shouldSkipNote)
                    {
                        if (_data.CanSwitchHands)
                        {
                            if (_data.CanHaveBlock && notePriority < _blockPriority &&
                                Mathf.Abs(notePriority - _blockPriority) > 20)
                            {
                                note = note.SetToBlock();
                                _blockPriority++;
                                if (_blockAdd > 0)
                                {
                                    _blockAdd--;
                                    if (_blockAdd == 0)
                                    {
                                        _blockPriority = notePriority;
                                    }
                                }
                                else
                                {
                                    _blockAdd = 3;
                                }
                            }
                            else if (notePriority < _leftSidePriority && Mathf.Abs(notePriority - _leftSidePriority) > 10)
                            {
                                note = note.SetType(HitSideType.Left);
                                _leftSidePriority++;
                                if (_leftSideAdd > 0)
                                {
                                    _leftSideAdd--;
                                    if (_leftSideAdd == 0)
                                    {
                                        _leftSidePriority = notePriority;
                                    }
                                }
                                else
                                {
                                    _leftSideAdd = 3;
                                }
                            }
                            else if (notePriority < _rightSidePriority &&
                                     Mathf.Abs(notePriority - _rightSidePriority) > 10)
                            {
                                note = note.SetType(HitSideType.Right);
                                _rightSidePriority++;
                                if (_rightSideAdd > 0)
                                {
                                    _rightSideAdd--;
                                    if (_rightSideAdd == 0)
                                    {
                                        _rightSidePriority = notePriority;
                                    }
                                }
                                else
                                {
                                    _rightSideAdd = 3;
                                }
                            }
                        }

                        if (!_thisSequence.HasObstacle)
                        {
                            note = note.SetCutDirection(ChoreographyNote.CutDirection.Jab);
                            note = note.SetLineLayer(ChoreographyNote.LineLayerType.Low);

                            switch (_thisSequence.Obstacle.HitSideType)
                            {
                                case HitSideType.Block:
                                    break;
                                case HitSideType.Left:
                                    note = note.SetLineIndex(2);
                                    break;
                                case HitSideType.Right:
                                    note = note.SetLineIndex(1);
                                    break;
                                default:
                                    return;
                            }
                        }

                        if (note.LineLayer == ChoreographyNote.LineLayerType.Low)
                        {
                            if (note.CutDir == ChoreographyNote.CutDirection.Uppercut)
                            {
                                note = note.SetToBasicJab();
                            }
                            else if (note.CutDir == ChoreographyNote.CutDirection.UppercutLeft ||
                                     note.CutDir == ChoreographyNote.CutDirection.UppercutRight)
                            {
                                note = note.SetToBlock();
                            }
                        }
                        else if (note.LineLayer == ChoreographyNote.LineLayerType.High)
                        {
                            if (note.CutDir == ChoreographyNote.CutDirection.JabDown ||
                                note.CutDir == ChoreographyNote.CutDirection.HookLeftDown ||
                                note.CutDir == ChoreographyNote.CutDirection.HookRightDown)
                            {
                                note = note.SetToBlock();
                            }
                        }

                        if (_data.DifficultyRank < 5)
                        {
                            if (note.LineLayer == ChoreographyNote.LineLayerType.High)
                            {
                                if (_data.DifficultyRank == 1 &&
                                    note.CutDir == ChoreographyNote.CutDirection.Jab)
                                {
                                    note = note.SetToBlock();
                                }
                                else if (note.CutDir == ChoreographyNote.CutDirection.JabDown ||
                                         note.CutDir == ChoreographyNote.CutDirection.HookLeftDown ||
                                         note.CutDir == ChoreographyNote.CutDirection.HookRightDown)
                                {
                                    note = note.SetToBasicJab();
                                }
                            }
                        }

                        switch (note.HitSideType)
                        {
                            case HitSideType.Block:
                                note = note.SetLineIndex(1);

                                _blockPriority--;
                                break;
                            case HitSideType.Left:
                                if (note.LineIndex > 1 ||
                                    (note.CutDir == ChoreographyNote.CutDirection.Jab && note.LineIndex != 1))
                                {
                                    note = note.SetLineIndex(1);
                                }

                                if (note.CutDir == ChoreographyNote.CutDirection.HookLeft)
                                {
                                    note = note.SetToBasicJab();
                                }

                                _leftSidePriority--;
                                break;
                            case HitSideType.Right:
                                if (note.LineIndex < 2 ||
                                    (note.CutDir == ChoreographyNote.CutDirection.Jab && note.LineIndex != 2))
                                {
                                    note = note.SetLineIndex(2);
                                }

                                if (note.CutDir == ChoreographyNote.CutDirection.HookRight)
                                {
                                    note = note.SetToBasicJab();
                                }

                                _rightSidePriority--;
                                break;
                        }

                        _thisSequence = _thisSequence.SetNote(note);
                    }
                }
                else if (formation.HasObstacle && !_thisSequence.HasObstacle)
                {
                    var obstacle = _thisSequence.Obstacle;
                    if (_thisSequence.HasNote)
                    {
                        var tempNote = _thisSequence.Note;
                        switch (tempNote.HitSideType)
                        {
                            case HitSideType.Block:
                                break;
                            case HitSideType.Left:
                                if (obstacle.LineIndex < 2)
                                {
                                    tempNote = tempNote.SetLineIndex(2);
                                }

                                break;
                            case HitSideType.Right:
                                if (obstacle.LineIndex > 1)
                                {
                                    tempNote = tempNote.SetLineIndex(1);
                                }

                                break;
                            default:
                                return;
                        }

                        tempNote = tempNote.SetCutDirection(ChoreographyNote.CutDirection.Jab);
                        tempNote = tempNote.SetLineLayer(ChoreographyNote.LineLayerType.Low);
                        _thisSequence = _thisSequence.SetNote(tempNote);
                    }

                    _thisSequence = _thisSequence.SetObstacle(obstacle);
                }
                else if (formation.HasEvent && !_thisSequence.HasEvent)
                {
                    if (_lastRotation + _data.MinTargetDistance * 5 < formation.Time)
                    {
                        _thisSequence = _thisSequence.SetEvent(formation.Event);
                        _lastRotation = formation.Time;
                    }
                }

                _lastSequence = formation;
            }
            else if (Mathf.Abs(_lastRotation - formation.Time) < .01f)
            {
                if (formation.HasEvent && !_thisSequence.HasEvent)
                {
                    _thisSequence = _thisSequence.SetEvent(formation.Event);
                }
            }

            if ((index + 1 < _sequenceables.Length && _sequenceables[1 + index].Time > _lastTime) || index + 1 == _sequenceables.Length)
            {
                _finalFormations.Add(_thisSequence);

                _thisSequence = new ChoreographyFormation();
            }
        }

        public struct Data
        {
            public bool CanSwitchHands { get; private set; }
            public bool CanHaveBlock { get; private set; }
            public int DifficultyRank { get; private set; }
            public float MinTargetDistance { get; private set; }

            public Data(float minTargetDistance, bool canSwitchHands, bool canHaveBlock, int difficultyRank)
            {
                MinTargetDistance = minTargetDistance;
                CanSwitchHands = canSwitchHands;
                CanHaveBlock = canHaveBlock;
                DifficultyRank = difficultyRank;
            }
        }
    }
}