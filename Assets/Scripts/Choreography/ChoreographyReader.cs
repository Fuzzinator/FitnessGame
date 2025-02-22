using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameModeManagement;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using static ChoreographyNote;

public class ChoreographyReader : MonoBehaviour, IOrderedInitialize
{
    public static ChoreographyReader Instance { get; private set; }
    public bool Initialized { get; private set; }

    [SerializeField]
    private Choreography _choreography;

    [SerializeField]
    private TransitionController _transitionController;

    private DifficultyInfo _difficultyInfo;

    private List<ChoreographyFormation> _sequenceables;
    private List<ChoreographyFormation> _formations;
    private List<ChoreographyFormation> _formationsThisTime;

    private int _formationObsCount = 0;

    public ChoreographyNote[] Notes => _choreography.Notes;
    public ChoreographyEvent[] Events => _choreography.Events;
    public ChoreographyObstacle[] Obstacles => _choreography.Obstacles;

    public bool CanHaveBlock =>
        PlaylistManager.Instance.TargetGameMode is not GameMode.JabsOnly or GameMode.OneHanded or GameMode.LightShow
        && !PlaylistManager.Instance.ForceOneHanded && !PlaylistManager.Instance.ForceJabsOnly;
    public bool CanSwitchHands => PlaylistManager.Instance.TargetGameMode is not GameMode.OneHanded
        && !PlaylistManager.Instance.ForceOneHanded;

    public float TimeToPoint { get; set; }


    [Header("Settings")]
    [SerializeField]
    private string _leftHandedMode = "LeftHanded";
    public UnityEvent finishedLoadingSong = new UnityEvent();

    private CancellationTokenSource _cancellationSource;

    private const float ThirtyfiveSeconds = 0.58333f;
    private const float ThreeAndAThirdSeconds = 0.05549f;
    private const float ThirdOfASecond = 0.005499f;
    private const string StanceChangeFrequency = "StanceChangeFrequency";
    private const string ChangeStanceDuringSongs = "ChangeStanceDuringSongs";
    private const string AllowTargetsDuringObstacles = "AllowTargetsDuringObstacles";

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

    public void Initialize()
    {
        if (Initialized)
        {
            return;
        }

        _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

        _sequenceables = new List<ChoreographyFormation>(5000);
        _formations = new List<ChoreographyFormation>(5000);
        _formationsThisTime = new List<ChoreographyFormation>(10);
        Initialized = true;
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
        if (playlist == null)
        {
            Debug.LogError("Current playlist is null. This is game breaking");
            return;
        }

        var gameMode = PlaylistManager.Instance.TargetGameMode;
        var targetDifficulty = PlaylistManager.Instance.TargetDifficulty;

        _difficultyInfo = item.SongInfo.TryGetActiveDifficultyInfo(targetDifficulty, gameMode);

        if (_cancellationSource.IsCancellationRequested)
        {
            _cancellationSource.Dispose();
            _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        }

        _choreography = await Choreography.AsyncLoadFromPlaylist(item, _difficultyInfo, _cancellationSource.Token);
        if (_choreography == null)
        {
            var visuals = new Notification.NotificationVisuals($"Failed to load song: {item.SongName}. The file may be missing.", "Loading Song Failed", "Return To Menu", "Try Next Song");
            NotificationManager.RequestNotification(visuals, ReturnToMenu, LevelManager.Instance.LoadNextSong);
            return;
        }

        finishedLoadingSong?.Invoke();
    }

    private void ReturnToMenu()
    {
        GameStateManager.Instance.SetState(GameState.Playing);
        PlaylistManager.Instance.FullReset();
        _transitionController.RequestTransition();
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
            _formationObsCount = 0;
            UpdateFormation();
            _choreography = new Choreography();
        }

        return _formations;
    }

    private void GetChoreographSequenceables()
    {
        _sequenceables.Clear();

        if (PlaylistManager.Instance.CurrentPlaylist == null)
        {
            Debug.LogError("Current playlist is null. This is game breaking");
            return;
        }

        var targetGameMode = PlaylistManager.Instance.TargetGameMode;

        var notes = Notes;
        var obstacles = Obstacles;
        var beatsTime = SongInfoReader.Instance.BeatsPerMinute;
        var songLength = (SongInfoReader.Instance.songInfo.SongLength / 60) * beatsTime;

        var changeStanceMidSong = SettingsManager.GetSetting(ChangeStanceDuringSongs, true);
        var stanceChangeFrequency = changeStanceMidSong ? SettingsManager.GetSetting(StanceChangeFrequency, 1f) * beatsTime : 1f;


        if (SettingsManager.GetCachedBool(_leftHandedMode, false))
        {
            notes = Choreography.SwapNotesSide(notes);
        }
        switch (targetGameMode)
        {
            case GameMode.Unset:
                break;
            case GameMode.Normal:
                break;
            case GameMode.JabsOnly:
                notes = Choreography.SetNotesToType(notes, CutDirection.Jab);
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
        if (PlaylistManager.Instance.ForceOneHanded)
        {
            notes = Choreography.SetNotesToSide(notes, HitSideType.Right);
        }
        if (PlaylistManager.Instance.ForceJabsOnly)
        {
            notes = Choreography.SetNotesToType(notes, CutDirection.Jab);
        }
        if (PlaylistManager.Instance.ForceNoObstacles)
        {
            obstacles = null;
        }
        for (var i = 0; i < (notes?.Length ?? 0); i++)
        {
            _sequenceables.Add(new ChoreographyFormation(notes[i]));
        }
        for (var i = 0; i < (obstacles?.Length ?? 0); i++)
        {
            _sequenceables.Add(new ChoreographyFormation(obstacles[i]));
        }
        var swapCount = 0f;
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

            if (changeStanceMidSong && songLength > stanceChangeFrequency)
            {
                var leftoverSeconds = songLength % stanceChangeFrequency;
                swapCount = (songLength / stanceChangeFrequency) + (leftoverSeconds > ThirtyfiveSeconds * beatsTime ? 0 : -1);
                for (var i = 1; i <= swapCount; i++)
                {
                    var swapFootingEvent = new ChoreographyEvent(i * stanceChangeFrequency, ChoreographyEvent.EventType.ChangeFooting, ChoreographyEvent.RotateEventValue.ClockWise60);
                    _sequenceables.Add(new ChoreographyFormation(swapFootingEvent));
                }
            }
        }
        var modedThirdSecond = ThirdOfASecond * beatsTime;
        var modedThreeAndThirdSeconds = ThreeAndAThirdSeconds * beatsTime;
        _sequenceables.Sort((sequenceable0, sequenceable1) => sequenceable0.Time.CompareTo(sequenceable1.Time));

        if (swapCount > 0)
        {
            for (var i = 1; i <= swapCount; i++)
            {
                var firstToRemoveIndex = _sequenceables.FindIndex((sequenceable) => sequenceable.Time >= (i * stanceChangeFrequency) - modedThirdSecond);
                if (firstToRemoveIndex > 0)
                {
                    var nextTime = _sequenceables[firstToRemoveIndex].Time;
                    var timeCap = (i * stanceChangeFrequency) + modedThreeAndThirdSeconds;
                    while (nextTime < timeCap)
                    {
                        if (_sequenceables[firstToRemoveIndex].HasEvent && _sequenceables[firstToRemoveIndex].Event.Type == ChoreographyEvent.EventType.ChangeFooting)
                        {
                            firstToRemoveIndex++;

                            if (firstToRemoveIndex >= _sequenceables.Count)
                            {
                                break;
                            }
                            nextTime = _sequenceables[firstToRemoveIndex].Time;
                            continue;
                        }
                        _sequenceables.RemoveAt(firstToRemoveIndex);

                        if (firstToRemoveIndex >= _sequenceables.Count)
                        {
                            break;
                        }
                        nextTime = _sequenceables[firstToRemoveIndex].Time;
                    }
                }
            }
        }
    }

    private void UpdateFormation()
    {
        if (_formations == null)
        {
            _formations = new List<ChoreographyFormation>(10000);
        }
        _formations.Clear();
        _formationsThisTime.Clear();

        var lastTime = -1f;

        ChoreographyFormation lastSequence = new ChoreographyFormation();
        ChoreographyFormation thisSequence = new ChoreographyFormation();

        float lastRotation = -1f;
        ChoreographyNote lastNote = new ChoreographyNote();
        lastNote = lastNote.SetCutDirection(CutDirection.Jab);
        var leftSidePriority = 0;
        var leftSideAdd = 0;
        var rightSidePriority = 0;
        var rightSideAdd = 0;
        var blockPriority = 0;
        var blockAdd = 0;
        var superNotePriority = 0;

        var minTargetDistance = _difficultyInfo.MinTargetSpace;

        var beatsTime = (60 / SongInfoReader.Instance.BeatsPerMinute);
        for (var i = 0; i < _sequenceables.Count; i++)
        {
            var sequenceable = _sequenceables[i];
            var sequenceTime = sequenceable.Time * beatsTime * PlaylistManager.Instance.SongSpeedMod;
            if (sequenceTime - TimeToPoint <= 0 || sequenceTime + TimeToPoint > PlaylistManager.Instance.CurrentSongLength)
            {
                continue;
            }
            //Used to determine minimum time between notes and if this current target is far enough apart.
            if (lastTime < sequenceable.Time)
            {
                if (!lastSequence.IsValid)
                {
                    lastTime = sequenceable.Time;
                    _formationsThisTime.Add(sequenceable);
                }
                else
                {
                    var minGap = minTargetDistance;
                    if (sequenceable.HasObstacle)
                    {
                        if (lastSequence.HasNote && lastSequence.Note.IsDirectional)
                        {
                            minGap *= 3f;
                        }
                        else
                        {
                            minGap *= 2;
                        }
                    }
                    else if (sequenceable.HasNote)
                    {
                        var note = sequenceable.Note;
                        if (note.CutDir != ChoreographyNote.CutDirection.Jab &&
                            note.HitSideType != HitSideType.Block)
                        {
                            minGap *= 1.5f;

                        }
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
                            if (chorEvent.Type != ChoreographyEvent.EventType.ChangeFooting)
                            {
                                //For now TODO: Lighting events.
                                continue;
                            }
                        }
                    }

                    var allowTargetObstacles = SettingsManager.GetCachedBool(AllowTargetsDuringObstacles, true);

                    if ((sequenceable.HasNote || sequenceable.HasObstacle) && lastTime + minGap < sequenceable.Time)
                    {
                        lastTime = sequenceable.Time;
                        _formationsThisTime.Add(sequenceable);
                    }
                    else if (sequenceable.HasObstacle)
                    {
                        if (lastSequence.HasNote && (Mathf.Approximately(sequenceable.Time, lastSequence.Time) || lastTime + minGap > sequenceable.Time))
                        {
                            if (_formationsThisTime.Count > 0)
                            {
                                _formationsThisTime.RemoveAt(_formationsThisTime.Count - 1);
                            }
                            _formations.RemoveAt(_formations.Count - 1);

                            var obstacle = new ChoreographyObstacle(lastSequence.Time, 1, sequenceable.Obstacle.Type, sequenceable.Obstacle.LineIndex, sequenceable.Obstacle.Width);
                            sequenceable = sequenceable.SetObstacle(obstacle);

                            thisSequence = new ChoreographyFormation(lastSequence.Note);

                            if (_difficultyInfo.DifficultyRank < 5 || !allowTargetObstacles)
                            {
                                switch (thisSequence.Note.HitSideType)
                                {
                                    case HitSideType.Left:
                                        leftSidePriority++;
                                        break;
                                    case HitSideType.Right:
                                        rightSidePriority++;
                                        break;
                                    case HitSideType.Block:
                                        blockPriority++;
                                        break;
                                }

                                thisSequence = thisSequence.RemoveNote();
                            }
                        }
                        lastTime = sequenceable.Time;
                        _formationsThisTime.Add(sequenceable);
                    }
                    else if (!sequenceable.HasEvent)
                    {
                        continue;
                    }
                }
            }

            if (Mathf.Approximately(lastTime, sequenceable.Time)) //if lastTime == sequenceable.Time but for floats
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

                    var allowTargetObstacles = SettingsManager.GetCachedBool(AllowTargetsDuringObstacles, true);

                    if (_difficultyInfo.DifficultyRank < 5 || !allowTargetObstacles)
                    {
                        shouldSkipNote = shouldSkipNote || thisSequence.HasObstacle;
                    }

                    if (!shouldSkipNote)
                    {
                        if (note.DirectionalDownCutRatio || note.DirectionalUpCutRatio)
                        {
                            if (PlaylistManager.Instance.TargetGameMode is not GameMode.JabsOnly)
                            {
                                note = note.SetHook();
                            }
                        }

                        if (lastSequence.HasObstacle && lastSequence.Obstacle.Type == ChoreographyObstacle.ObstacleType.Crouch)
                        {
                            if (!PlaylistManager.Instance.ForceJabsOnly)
                            {
                                switch (note.LineLayer)
                                {
                                    case LineLayerType.Low:
                                        note = note.SetToBasicJab();
                                        break;
                                    case LineLayerType.Middle:
                                        note = note.SetCutDirection(CutDirection.Uppercut);
                                        break;
                                    case LineLayerType.High:
                                        note = note.SetLineLayer(LineLayerType.Middle);
                                        note = note.SetCutDirection(CutDirection.Uppercut);
                                        break;
                                }
                            }
                        }

                        if (CanSwitchHands)
                        {
                            if ((note.IsDirectional || lastNote.IsDirectional) && lastNote.HitSideType == HitSideType.Right)
                            {
                                note = note.SetType(HitSideType.Left);
                                note = note.SetPreventSwitchingSides(true);
                                leftSidePriority++;
                            }
                            else if ((note.IsDirectional || lastNote.IsDirectional) && lastNote.HitSideType == HitSideType.Left)
                            {
                                note = note.SetType(HitSideType.Right);
                                note = note.SetPreventSwitchingSides(true);
                                rightSidePriority++;
                            }
                            else if (CanHaveBlock && notePriority < blockPriority &&
                                Mathf.Abs(notePriority - blockPriority) > 20)
                            {
                                note = note.SetToBlock();
                                note = note.SetPreventSwitchingSides(true);
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
                            else if (notePriority < leftSidePriority && Mathf.Abs(notePriority - leftSidePriority) > 3)
                            {
                                note = note.SetType(HitSideType.Left);
                                note = note.SetPreventSwitchingSides(true);
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
                            else if (notePriority < rightSidePriority && Mathf.Abs(notePriority - rightSidePriority) > 3)

                            {
                                note = note.SetType(HitSideType.Right);
                                note = note.SetPreventSwitchingSides(true);
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

                        if (thisSequence.HasObstacle)
                        {
                            if (_difficultyInfo.DifficultyRank >= 5)
                            {
                                note = note.SetCutDirection(ChoreographyNote.CutDirection.Jab);
                                note = note.SetLineLayer(ChoreographyNote.LineLayerType.Low);

                                switch (thisSequence.Obstacle.HitSideType)
                                {
                                    case HitSideType.Block:
                                        break;
                                    case HitSideType.Left:
                                        note = note.SetLineIndex(2);
                                        if (!note.PreventSwitchingSides)
                                        {
                                            note = (note.HitSideType == HitSideType.Block ? note.SetType(HitSideType.Left) : note);
                                        }
                                        break;
                                    case HitSideType.Right:
                                        note = note.SetLineIndex(1);
                                        if (!note.PreventSwitchingSides)
                                        {
                                            note = (note.HitSideType == HitSideType.Block ? note.SetType(HitSideType.Right) : note);
                                        }
                                        break;
                                    default:
                                        continue;
                                }
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
                                if (CanHaveBlock)
                                {
                                    note = note.SetToBlock();
                                }
                                else
                                {
                                    var targetLine = thisSequence.HasObstacle ? LineLayerType.Low : LineLayerType.Middle;
                                    note = note.SetToBasicJab(targetLine);
                                }
                            }
                        }
                        else if (note.LineLayer == ChoreographyNote.LineLayerType.High)
                        {
                            if (note.CutDir == ChoreographyNote.CutDirection.JabDown ||
                                note.CutDir == ChoreographyNote.CutDirection.HookLeftDown ||
                                note.CutDir == ChoreographyNote.CutDirection.HookRightDown)
                            {
                                if (CanHaveBlock)
                                {
                                    note = note.SetToBlock();
                                }
                                else
                                {
                                    var targetLine = thisSequence.HasObstacle ? LineLayerType.Low : LineLayerType.Middle;
                                    note = note.SetToBasicJab(targetLine);
                                }
                            }
                        }

                        if (_difficultyInfo.DifficultyRank < 5)
                        {
                            if (note.LineLayer == ChoreographyNote.LineLayerType.High)
                            {
                                if (_difficultyInfo.DifficultyRank == 1 &&
                                    note.CutDir == ChoreographyNote.CutDirection.Jab && CanHaveBlock)
                                {
                                    note = note.SetToBlock();
                                }
                                else if (note.CutDir == ChoreographyNote.CutDirection.JabDown ||
                                         note.CutDir == ChoreographyNote.CutDirection.HookLeftDown ||
                                         note.CutDir == ChoreographyNote.CutDirection.HookRightDown || !CanHaveBlock)
                                {
                                    var targetLine = thisSequence.HasObstacle ? LineLayerType.Low : LineLayerType.Middle;
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
                                    note = note.SetCutDirection(ChoreographyNote.CutDirection.HookRight);
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
                                    note = note.SetCutDirection(ChoreographyNote.CutDirection.HookLeft);
                                }

                                rightSidePriority--;
                                break;
                        }
                        if (_difficultyInfo.DifficultyRank >= 5 || !thisSequence.HasObstacle)
                        {
                            thisSequence = thisSequence.SetNote(note);
                        }
                    }
                }
                else if (sequenceable.HasObstacle && !thisSequence.HasObstacle)
                {
                    var obstacle = sequenceable.Obstacle;
                    if (thisSequence.HasNote)
                    {
                        var targetObstacles = SettingsManager.GetCachedBool(AllowTargetsDuringObstacles, true);

                        if (_difficultyInfo.DifficultyRank < 5 || !targetObstacles)
                        {
                            switch (thisSequence.Note.HitSideType)
                            {
                                case HitSideType.Left:
                                    leftSidePriority++;
                                    break;
                                case HitSideType.Right:
                                    rightSidePriority++;
                                    break;
                                case HitSideType.Block:
                                    blockPriority++;
                                    break;
                            }
                            thisSequence = thisSequence.RemoveNote();
                        }
                        else
                        {
                            var note = thisSequence.Note;
                            switch (note.HitSideType)
                            {
                                case HitSideType.Block:
                                    break;
                                case HitSideType.Left:
                                    if (obstacle.LineIndex < 2)
                                    {
                                        note = note.SetLineIndex(2);
                                        if (!note.PreventSwitchingSides)
                                        {
                                            note = (note.HitSideType == HitSideType.Block ? note.SetType(HitSideType.Left) : note);
                                        }
                                    }

                                    break;
                                case HitSideType.Right:
                                    if (obstacle.LineIndex > 1)
                                    {
                                        note = note.SetLineIndex(1);
                                        if (!note.PreventSwitchingSides)
                                        {
                                            note = (note.HitSideType == HitSideType.Block ? note.SetType(HitSideType.Right) : note);
                                        }
                                    }

                                    break;
                                default:
                                    continue;
                            }

                            note = note.SetCutDirection(ChoreographyNote.CutDirection.Jab);
                            note = note.SetLineLayer(ChoreographyNote.LineLayerType.Low);
                            thisSequence = thisSequence.SetNote(note);
                        }
                    }

                    thisSequence = thisSequence.SetObstacle(obstacle);

                    var allowTargetObstacles = SettingsManager.GetCachedBool(AllowTargetsDuringObstacles, true);

                    if (_difficultyInfo.DifficultyRank < 5 || !allowTargetObstacles)
                    {
                        thisSequence.RemoveNote();
                    }
                }
                else if (sequenceable.HasEvent && !thisSequence.HasEvent)
                {
                    if (lastRotation + minTargetDistance * 5 < sequenceable.Time)
                    {
                        thisSequence = thisSequence.SetEvent(sequenceable.Event);
                        lastRotation = sequenceable.Time;
                    }
                }
            }
            else if (Mathf.Abs(lastRotation - sequenceable.Time) < .01f || (sequenceable.HasEvent && sequenceable.Event.Type == ChoreographyEvent.EventType.ChangeFooting))
            {
                if (sequenceable.HasEvent && !thisSequence.HasEvent)
                {
                    thisSequence = thisSequence.SetEvent(sequenceable.Event);
                }
            }




            if ((i + 1 < _sequenceables.Count && _sequenceables[1 + i].Time > lastTime) || i + 1 == _sequenceables.Count)
            {
                //Check if should convert to super note
                var isSuperNote = false;

                if (thisSequence.HasNote && thisSequence.Note.HitSideType != HitSideType.Block)
                {
                    if (Mathf.Abs(lastSequence.Time - thisSequence.Time) > minTargetDistance * 2)
                    {
                        var targetDifficulty = PlaylistManager.Instance.TargetDifficulty;
                        var minSpace = 6 - (int)targetDifficulty;
                        var note = thisSequence.Note;

                        if (superNotePriority > 20)
                        {
                            isSuperNote = true;
                            thisSequence.SetNote(thisSequence.Note.SetSuperNote(isSuperNote));
                            superNotePriority = 0;
                        }
                        else if (superNotePriority >= minSpace && !(note.HitSideType is HitSideType.Unused or HitSideType.Block))
                        {
                            for (var j = 0; j < _formationsThisTime.Count; j++)
                            {
                                var formationA = _formationsThisTime[j];
                                if (formationA.HasNote && formationA.Note.TypeMatches(formationA.Note))
                                {
                                    isSuperNote = true;
                                    thisSequence.SetNote(thisSequence.Note.SetSuperNote(isSuperNote));
                                    superNotePriority = 0;
                                    break;
                                }
                            }
                        }
                    }
                    if (!isSuperNote)
                    {
                        superNotePriority++;
                    }
                }

                if(thisSequence.HasNote && thisSequence.Note.IsDirectional && lastSequence.HasNote && lastNote.IsDirectional && thisSequence.Note.HitSideType == lastNote.HitSideType)
                {
                    Debug.Log($"{i}BAD!");
                }

                _formations.Add(thisSequence);

                if (thisSequence.HasObstacle)
                {
                    _formationObsCount++;
                }

                if (thisSequence.HasNote)
                {
                    lastNote = thisSequence.Note;
                }

                lastSequence = thisSequence;
                thisSequence = new ChoreographyFormation();
                _formationsThisTime.Clear();
            }
        }
    }
}