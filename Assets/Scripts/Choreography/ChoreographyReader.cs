using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameModeManagement;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ChoreographyReader : MonoBehaviour
{
    public static ChoreographyReader Instance { get; private set; }


    [SerializeField] private Choreography _choreography;

    private DifficultyInfo _difficultyInfo;

    private List<ISequenceable> _sequenceables;
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

        _sequenceables = new List<ISequenceable>(5000);
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
            _sequenceables.Add(notes[i]);
        }
        for (var i = 0; i < (obstacles?.Length??0); i++)
        {
            _sequenceables.Add(obstacles[i]);
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
                            _sequenceables.Add(Events[i]);
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

        ISequenceable thisTimeNote = null;
        ISequenceable thisTimeObstacle = null;
        ISequenceable thisTimeEvent = null;

        ISequenceable lastSequenceable = null;
        float lastRotation = 0f;
        var leftSidePriority = 0;
        var leftSideAdd = 0;
        var rightSidePriority = 0;
        var rightSideAdd = 0;
        var blockPriority = 0;
        var blockAdd = 0;

        var minTargetDistance = _difficultyInfo.MinTargetSpace;
        for (var i = 0; i < _sequenceables.Count; i++)
        {
            var sequenceable = _sequenceables[i];
            if (lastTime < sequenceable.Time)
            {
                if (lastSequenceable == null)
                {
                    lastTime = sequenceable.Time;
                }
                else
                {
                    var minGap = minTargetDistance;
                    if (sequenceable is ChoreographyNote note)
                    {
                        if (note.CutDir != ChoreographyNote.CutDirection.Jab &&
                            note.HitSideType != HitSideType.Block)
                        {
                            minGap *= 1.5f;
                        }
                    }
                    else if (sequenceable is ChoreographyObstacle obstacle)
                    {
                        minGap *= 2;
                    }
                    else if (sequenceable is ChoreographyEvent chorEvent)
                    {
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

                    if (sequenceable is ChoreographyNote or ChoreographyObstacle)
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
                if (sequenceable is ChoreographyNote note)
                {
                    var notePriority = note.HitSideType switch
                    {
                        HitSideType.Block => blockPriority,
                        HitSideType.Left => leftSidePriority,
                        HitSideType.Right => rightSidePriority,
                        _ => 0
                    };

                    var shouldSkipNote = thisTimeNote != null;
                    if (shouldSkipNote)
                    {
                        var currentNote = (ChoreographyNote) thisTimeNote;
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
                                note.SetToBlock();
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
                                note.SetType(HitSideType.Left);
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
                                note.SetType(HitSideType.Right);
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

                        if (thisTimeObstacle != null)
                        {
                            note.SetCutDirection(ChoreographyNote.CutDirection.Jab);
                            note.SetLineLayer(ChoreographyNote.LineLayerType.Low);

                            switch (thisTimeObstacle.HitSideType)
                            {
                                case HitSideType.Block:
                                    break;
                                case HitSideType.Left:
                                    note.SetLineIndex(2);
                                    break;
                                case HitSideType.Right:
                                    note.SetLineIndex(1);
                                    break;
                                default:
                                    continue;
                            }
                        }

                        if (note.LineLayer == ChoreographyNote.LineLayerType.Low)
                        {
                            if (note.CutDir == ChoreographyNote.CutDirection.Uppercut)
                            {
                                note.SetToBasicJab();
                            }
                            else if (note.CutDir == ChoreographyNote.CutDirection.UppercutLeft ||
                                     note.CutDir == ChoreographyNote.CutDirection.UppercutRight)
                            {
                                note.SetToBlock();
                            }
                        }
                        else if (note.LineLayer == ChoreographyNote.LineLayerType.High)
                        {
                            if (note.CutDir == ChoreographyNote.CutDirection.JabDown ||
                                note.CutDir == ChoreographyNote.CutDirection.HookLeftDown ||
                                note.CutDir == ChoreographyNote.CutDirection.HookRightDown)
                            {
                                note.SetToBlock();
                            }
                        }

                        if (_difficultyInfo.DifficultyRank < 5)
                        {
                            if (note.LineLayer == ChoreographyNote.LineLayerType.High)
                            {
                                if (_difficultyInfo.DifficultyRank == 1 &&
                                    note.CutDir == ChoreographyNote.CutDirection.Jab)
                                {
                                    note.SetToBlock();
                                }
                                else if (note.CutDir == ChoreographyNote.CutDirection.JabDown ||
                                         note.CutDir == ChoreographyNote.CutDirection.HookLeftDown ||
                                         note.CutDir == ChoreographyNote.CutDirection.HookRightDown)
                                {
                                    note.SetToBasicJab();
                                }
                            }
                        }

                        switch (note.HitSideType)
                        {
                            case HitSideType.Block:
                                note.SetLineIndex(1);

                                blockPriority--;
                                break;
                            case HitSideType.Left:
                                if (note.LineIndex > 1 ||
                                    (note.CutDir == ChoreographyNote.CutDirection.Jab && note.LineIndex != 1))
                                {
                                    note.SetLineIndex(1);
                                }

                                if (note.CutDir == ChoreographyNote.CutDirection.HookLeft)
                                {
                                    note.SetToBasicJab();
                                }

                                leftSidePriority--;
                                break;
                            case HitSideType.Right:
                                if (note.LineIndex < 2 ||
                                    (note.CutDir == ChoreographyNote.CutDirection.Jab && note.LineIndex != 2))
                                {
                                    note.SetLineIndex(2);
                                }

                                if (note.CutDir == ChoreographyNote.CutDirection.HookRight)
                                {
                                    note.SetToBasicJab();
                                }

                                rightSidePriority--;
                                break;
                        }

                        thisTimeNote = note;
                    }
                }
                else if (sequenceable is ChoreographyObstacle obstacle && thisTimeObstacle == null)
                {
                    if (thisTimeNote is ChoreographyNote tempNote)
                    {
                        switch (tempNote.HitSideType)
                        {
                            case HitSideType.Block:
                                break;
                            case HitSideType.Left:
                                if (obstacle.LineIndex < 2)
                                {
                                    tempNote.SetLineIndex(2);
                                }

                                break;
                            case HitSideType.Right:
                                if (obstacle.LineIndex > 1)
                                {
                                    tempNote.SetLineIndex(1);
                                }

                                break;
                            default:
                                continue;
                        }

                        tempNote.SetCutDirection(ChoreographyNote.CutDirection.Jab);
                        tempNote.SetLineLayer(ChoreographyNote.LineLayerType.Low);
                        thisTimeNote = tempNote;
                    }

                    thisTimeObstacle = obstacle;
                }

                lastSequenceable = sequenceable;
            }
            else if (Mathf.Abs(lastRotation - sequenceable.Time) < .01f)
            {
                if (sequenceable is ChoreographyEvent chorEvent && thisTimeEvent == null)
                {
                    thisTimeEvent = sequenceable;
                }
            }

            if ((i + 1 < _sequenceables.Count && _sequenceables[1 + i].Time > lastTime) || i + 1 == _sequenceables.Count)
            {
                _formations.Add(new ChoreographyFormation(lastTime, note: thisTimeNote, obstacle: thisTimeObstacle,
                    choreographyEvent: thisTimeEvent));

                thisTimeNote = null;
                thisTimeObstacle = null;
                thisTimeEvent = null;
            }
        }
    }
}