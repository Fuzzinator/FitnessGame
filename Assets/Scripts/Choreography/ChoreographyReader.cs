using System;
using System.Collections.Generic;
using System.Linq;
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

    private const int MaxStreak = 2;
    private const float ThirtyfiveSeconds = 0.58333f;
    private const float ThreeAndAThirdSeconds = 0.05549f;
    private const float ThirdOfASecond = 0.005499f;
    private const string MaxStreakSetting = "MaxTargetStreak";
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

    private void MergeSequences()
    {
        if (_formations == null)
        {
            _formations = new List<ChoreographyFormation>(10000);
        }
        _formations.Clear();

        var allowTargetObstacles = SettingsManager.GetCachedBool(AllowTargetsDuringObstacles, true);
        var beatsTime = (60 / SongInfoReader.Instance.BeatsPerMinute) / PlaylistManager.Instance.SongSpeedMod;
        var lastSequenceTime = 0f;
        var lastRotationTime = 0f;
        //var lastTime = -1f;

        ChoreographyFormation thisSequence = new ChoreographyFormation();
        ChoreographyFormation lastSequence = new ChoreographyFormation();

        for (var i = 0; i < _sequenceables.Count; i++)
        {
            var sequenceable = _sequenceables[i];
            var sequenceTime = sequenceable.Time * beatsTime;
            if (sequenceTime - TimeToPoint <= 0 || sequenceTime + TimeToPoint > PlaylistManager.Instance.CurrentSongLength)
            {
                continue;
            }

            if (thisSequence.IsValid && (!Mathf.Approximately(thisSequence.Time, sequenceable.Time) && thisSequence.Time < sequenceable.Time) && (_formations.Count == 0 || !Mathf.Approximately(_formations[^1].Time, thisSequence.Time)))
            {
                _formations.Add(thisSequence);
                lastSequence = thisSequence;
                thisSequence = new ChoreographyFormation();
            }

            if (!CanUseFormation(lastSequence, sequenceable, lastSequenceTime, sequenceTime, lastRotationTime))
            {
                if (allowTargetObstacles && sequenceable.HasObstacle && !lastSequence.HasObstacle)
                {
                    var obstacle = sequenceable.Obstacle;
                    obstacle = new ChoreographyObstacle(lastSequence.Time, obstacle.Duration, obstacle.Type, obstacle.LineIndex, obstacle.Width);
                    lastSequence = lastSequence.SetObstacle(obstacle);
                    _formations[^1] = lastSequence;
                }
                else if (!allowTargetObstacles && PlaylistManager.Instance.TargetGameMode == GameMode.LegDay)
                {
                    var obstacle = sequenceable.Obstacle;
                    obstacle = new ChoreographyObstacle(lastSequence.Time, obstacle.Duration, obstacle.Type, obstacle.LineIndex, obstacle.Width);
                    lastSequence = lastSequence.SetObstacle(obstacle);
                    lastSequence = lastSequence.RemoveNote();
                    _formations[^1] = lastSequence;
                }
                continue;
            }

            if (!thisSequence.IsValid || Mathf.Approximately(sequenceable.Time, thisSequence.Time))
            {
                if (!thisSequence.HasNote && sequenceable.HasNote)
                {
                    if (!thisSequence.HasObstacle || _difficultyInfo.DifficultyRank >= 5 && allowTargetObstacles)
                    {
                        thisSequence = thisSequence.SetNote(sequenceable.Note);
                    }
                }

                if (!thisSequence.HasObstacle && sequenceable.HasObstacle)
                {
                    if (thisSequence.HasNote && _difficultyInfo.DifficultyRank < 5 || !allowTargetObstacles)
                    {
                        thisSequence = thisSequence.RemoveNote();
                    }
                    thisSequence = thisSequence.SetObstacle(sequenceable.Obstacle);
                }

                if (!thisSequence.HasEvent && sequenceable.HasEvent)
                {
                    thisSequence = thisSequence.SetEvent(sequenceable.Event);

                    if (sequenceable.Event.Type is ChoreographyEvent.EventType.EarlyRotation
                    or ChoreographyEvent.EventType.LateRotation)
                    {
                        lastRotationTime = sequenceTime;
                    }
                }

                if (thisSequence.HasNote && thisSequence.HasObstacle && lastSequence.HasNote && lastSequence.Note.IsDirectional)
                {
                    if (thisSequence.Obstacle.Type == ChoreographyObstacle.ObstacleType.Dodge)
                    {
                        thisSequence = thisSequence.RemoveNote();
                    }
                }

                lastSequenceTime = sequenceTime;
            }
        }
        _sequenceables.Clear();
        _sequenceables.AddRange(_formations);
        _formations.Clear();
    }

    private void UpdateFormation()
    {
        MergeSequences();

        ChoreographyFormation lastSequence = new ChoreographyFormation();

        ChoreographyNote lastNote = new ChoreographyNote();
        lastNote = lastNote.SetCutDirection(CutDirection.Jab);
        var notePriorities = new SidePriority();

        var minTargetDistance = _difficultyInfo.MinTargetSpace;

        for (var i = 0; i < _sequenceables.Count; i++)
        {
            var formation = _sequenceables[i];
            //Used to determine minimum time between notes and if this current target is far enough apart.

            if (formation.HasNote)
            {
                var note = formation.Note;
                notePriorities.current = note.HitSideType switch
                {
                    HitSideType.Block => notePriorities.blockPriority,
                    HitSideType.Left => notePriorities.leftSidePriority,
                    HitSideType.Right => notePriorities.rightSidePriority,
                    _ => 0
                };

                if (note.DirectionalDownCutRatio || note.DirectionalUpCutRatio)
                {
                    if (PlaylistManager.Instance.TargetGameMode is not GameMode.JabsOnly)
                    {
                        note = note.SetHook();
                    }
                }

                note = HandlePostDuck(lastSequence, note);

                note = HandleSidePriority(note, lastNote, notePriorities);

                if (formation.HasObstacle)
                {
                    note = HandleNoteObstacles(formation.Obstacle.HitSideType, note);
                }

                if (note.LineLayer == ChoreographyNote.LineLayerType.Low)
                {
                    if (note.CutDir == ChoreographyNote.CutDirection.Uppercut)
                    {
                        note = note.SetLineLayer(LineLayerType.Middle);
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
                            if (formation.HasObstacle)
                            {
                                note = note.SetToBasicJab(LineLayerType.Low);
                            }
                            else
                            {
                                note = note.SetLineLayer(LineLayerType.Middle);
                            }
                        }
                    }
                }
                else if (note.LineLayer == ChoreographyNote.LineLayerType.High)
                {
                    if (_difficultyInfo.DifficultyRank < 5)
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
                            var targetLine = formation.HasObstacle ? LineLayerType.Low : LineLayerType.Middle;
                            note = note.SetToBasicJab();
                        }
                    }
                    else
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
                                var targetLine = formation.HasObstacle ? LineLayerType.Low : LineLayerType.Middle;
                                note = note.SetToBasicJab(targetLine);
                            }
                        }
                    }
                }

                switch (note.HitSideType)
                {
                    case HitSideType.Block:
                        note = note.SetLineIndex(1);

                        notePriorities.blockPriority--;
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

                        notePriorities.leftSidePriority--;
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

                        notePriorities.rightSidePriority--;
                        break;
                }

                //Check if should convert to super note
                var isSuperNote = false;

                if (note.HitSideType != HitSideType.Block)
                {
                    if (Mathf.Abs(lastSequence.Time - note.Time) > minTargetDistance * 2)
                    {
                        var targetDifficulty = PlaylistManager.Instance.TargetDifficulty;
                        var minSpace = 6 - (int)targetDifficulty;

                        if (notePriorities.superNotePriority > 20)
                        {
                            isSuperNote = true;
                            note = note.SetSuperNote(isSuperNote);
                            notePriorities.superNotePriority = 0;
                        }
                        else if (notePriorities.superNotePriority >= minSpace && !(note.HitSideType is HitSideType.Unused or HitSideType.Block))
                        {
                            isSuperNote = true;
                            note = note.SetSuperNote(isSuperNote);
                            notePriorities.superNotePriority = 0;
                        }
                    }
                    if (!isSuperNote)
                    {
                        notePriorities.superNotePriority++;
                    }
                }

                formation = formation.SetNote(note);
            }



            _formations.Add(formation);

            if (formation.HasObstacle)
            {
                _formationObsCount++;
            }

            if (formation.HasNote)
            {
                lastNote = formation.Note;
            }

            lastSequence = formation;
        }
    }

    private bool CanUseFormation(ChoreographyFormation lastSequence, ChoreographyFormation sequenceable, float lastTime, float sequenceTime, float lastRotation)
    {
        var minTargetDistance = _difficultyInfo.MinTargetSpace;
        if (!lastSequence.IsValid)
        {
            return true;
        }

        if (lastTime < sequenceTime)
        {
            var minGap = minTargetDistance;
            if (sequenceable.HasObstacle)
            {
                if (lastSequence.HasNote && lastSequence.Note.IsDirectional)
                {
                    minGap *= 1.5f;
                }
            }
            else if (sequenceable.HasNote)
            {
                var note = sequenceable.Note;
                if (note.CutDir != ChoreographyNote.CutDirection.Jab &&
                    note.HitSideType != HitSideType.Block)
                {
                    minGap *= 1.25f;

                }
            }
            else if (sequenceable.HasEvent)
            {
                var chorEvent = sequenceable.Event;
                if (chorEvent.Type is ChoreographyEvent.EventType.EarlyRotation
                    or ChoreographyEvent.EventType.LateRotation)
                {
                    minGap *= 5;
                    if (lastRotation + minGap < sequenceTime)
                    {
                        return true;
                    }
                }
                else
                {
                    return chorEvent.Type == ChoreographyEvent.EventType.ChangeFooting;
                    //For now TODO: Lighting events.
                }
            }


            if ((sequenceable.HasNote || sequenceable.HasObstacle) && lastTime + minGap < sequenceTime)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }

    private ChoreographyNote HandlePostDuck(ChoreographyFormation lastSequence, ChoreographyNote note)
    {
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
        return note;
    }

    private ChoreographyNote HandleSidePriority(ChoreographyNote note, ChoreographyNote lastNote, SidePriority notePriority)
    {
        if (!CanSwitchHands)
        {
            return note;
        }

        var maxStreak = SettingsManager.GetCachedInt(MaxStreakSetting, MaxStreak);

        if ((note.IsDirectional || lastNote.IsDirectional) && lastNote.HitSideType == HitSideType.Right)
        {
            note = note.SetType(HitSideType.Left);
            note = note.SetPreventSwitchingSides(1);
            //notePriority.leftSidePriority--;
        }
        else if ((note.IsDirectional || lastNote.IsDirectional) && lastNote.HitSideType == HitSideType.Left)
        {
            note = note.SetType(HitSideType.Right);
            note = note.SetPreventSwitchingSides(1);
            //notePriority.rightSidePriority--;
        }
        else if (CanHaveBlock && notePriority.current < notePriority.blockPriority &&
            Mathf.Abs(notePriority.current - notePriority.blockPriority) > 20)
        {
            note = note.SetToBlock();
            note = note.SetPreventSwitchingSides(1);
            notePriority.blockPriority++;
            if (notePriority.blockAdd > 0)
            {
                notePriority.blockAdd--;
                if (notePriority.blockAdd == 0)
                {
                    notePriority.blockPriority = notePriority.current;
                }
            }
            else
            {
                notePriority.blockAdd = maxStreak;
            }
        }
        else if (notePriority.current < notePriority.leftSidePriority && Mathf.Abs(notePriority.current - notePriority.leftSidePriority) > maxStreak)
        {
            note = note.SetType(HitSideType.Left);
            note = note.SetPreventSwitchingSides(1);
            notePriority.leftSidePriority++;
            if (notePriority.leftSideAdd > 0)
            {
                notePriority.leftSideAdd--;
                if (notePriority.leftSideAdd == 0)
                {
                    notePriority.leftSidePriority = notePriority.current;
                }
            }
            else
            {
                notePriority.leftSideAdd = maxStreak;
            }
        }
        else if (notePriority.current < notePriority.rightSidePriority && Mathf.Abs(notePriority.current - notePriority.rightSidePriority) > maxStreak)

        {
            note = note.SetType(HitSideType.Right);
            note = note.SetPreventSwitchingSides(1);
            notePriority.rightSidePriority++;
            if (notePriority.rightSideAdd > 0)
            {
                notePriority.rightSideAdd--;
                if (notePriority.rightSideAdd == 0)
                {
                    notePriority.rightSidePriority = notePriority.current;
                }
            }
            else
            {
                notePriority.rightSideAdd = maxStreak;
            }
        }

        return note;
    }


    private ChoreographyNote HandleNoteObstacles(HitSideType obstacleHitSideType, ChoreographyNote note)
    {
        note = note.SetCutDirection(ChoreographyNote.CutDirection.Jab);
        note = note.SetLineLayer(ChoreographyNote.LineLayerType.Low);

        switch (obstacleHitSideType)
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
        }
        return note;
    }

    private class SidePriority
    {
        public int current = 0;
        public int leftSidePriority = 0;
        public int leftSideAdd = 0;
        public int rightSidePriority = 0;
        public int rightSideAdd = 0;
        public int blockPriority = 0;
        public int blockAdd = 0;
        public int superNotePriority = 0;
    }
}