using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using GameModeManagement;
using SimpleTweens;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class ChoreographySequencer : MonoBehaviour
{
    [SerializeField]
    private ActiveLaneIndicator _laneIndicator;

    [Header("Sequence Positioning")]
    [SerializeField]
    private Transform _formationStart;

    [SerializeField]
    private Transform _formationEnd;

    [SerializeField]
    private Transform _playerCenter;

    [SerializeField]
    private Transform[] _sequenceStartPoses;

    [SerializeField]
    private Transform[] _sequenceEndPoses;

    [SerializeField]
    private OptimalStrikePositioning _optimalStrikePositioning;

    [SerializeField]
    private Transform _optimalStrikePoint;
    [SerializeField]
    private Transform _optimalJabStrikePoint;

    private float _meterDistance;
    private float _optimalPointDistance;
    private float _optimalJabPointDistance;
    private float _songStartTime;
    private float _delayStartTime;
    private float _pauseOffset;

    private float _currentRotation = 0;
    private const float MAX90ROTATION = 45;
    private const string LEFTHANDED = "LeftHanded";

    private ChoreographyFormation _lastFormation;

    private CancellationToken _cancellationToken;

    [SerializeField]
    private HitSideType _currentStance = HitSideType.Left;

    public HitSideType CurrentStance
    {
        get => _currentStance;
        set
        {
            _currentStance = value;
            var temp = (int)value;
            temp++;
            _stanceUpdated?.Invoke(temp);
        }
    }

    [Header("Events")]
    [SerializeField]
    private UnityEvent _sequenceStarted = new UnityEvent();

    [SerializeField]
    private UnityEvent<int> _stanceUpdated = new UnityEvent<int>();

    private bool _sequenceUnstartedOrFinished = true;
    private bool _resetting = false;

    private Dictionary<float, ActiveLaneIndicator> _laneIndicators = new Dictionary<float, ActiveLaneIndicator>(20);
    public bool SequenceRunning { get; private set; }
    

    // Start is called before the first frame update
    void Start()
    {

        var position = _formationStart.position;
        _meterDistance = Vector3.Distance(position, _formationEnd.position);
        _optimalStrikePositioning.OnPositionChanged.AddListener(RepositionStrikes);

        _sequenceUnstartedOrFinished = true;
        _cancellationToken = this.GetCancellationTokenOnDestroy();


    }

    private void OnEnable()
    {
        SetStartingFooting();
        GameStateManager.Instance.gameStateChanged.AddListener(GameStateListener);
    }

    private void OnDisable()
    {
        GameStateManager.Instance.gameStateChanged.RemoveListener(GameStateListener);
    }

    private void OnDestroy()
    {
        GameStateManager.Instance.gameStateChanged.RemoveListener(GameStateListener);
    }

    private void RepositionStrikes(float armLength)
    {
        var position = _formationStart.position;
        _optimalPointDistance = Vector3.Distance(position, _optimalStrikePositioning.DirectionalStrikePoint);
        _optimalJabPointDistance = Vector3.Distance(position, _optimalStrikePositioning.JabStrikePoint);
    }

    private void GameStateListener(GameState oldState, GameState newState)
    {
        if (oldState == GameState.Paused && newState == GameState.Playing)
        {
            if (SequenceRunning || _sequenceUnstartedOrFinished)
            {
                return;
            }

            ResumeChoreography();
        }
        else if (oldState == GameState.Playing && (newState == GameState.Paused || newState == GameState.Unfocused))
        {
            if (!SequenceRunning || _sequenceUnstartedOrFinished)
            {
                return;
            }

            PauseChoreography();
        }
    }

    private void SetStartingFooting()
    {
        CurrentStance = PlaylistManager.Instance.CurrentPlaylist.StartingSide;
    }

    public void InitializeSequence()
    {
        if (ChoreographyReader.Instance == null)
        {
            Debug.LogError("No ChoreographyReader");
            return;
        }
        RepositionStrikes(0f);

        var tweenSpeed = PlaylistManager.Instance.TargetSpeedMod * SongInfoReader.Instance.NoteSpeed / _meterDistance * 10;
        var timeToPoint = _optimalPointDistance / tweenSpeed;

        ChoreographyReader.Instance.TimeToPoint = timeToPoint;

        var formations = ChoreographyReader.Instance.GetOrderedFormations();
        if (formations == null || formations.Count <= 0)
        {
            return;
        }

        _songStartTime = Time.time;
        _delayStartTime = 0;
        _pauseOffset = 0;

        CreateSequence(formations[0], 1);
        SequenceRunning = true;
        _sequenceUnstartedOrFinished = false;
        LevelManager.Instance.SetChoreographyCompleted(false);
    }

    private void CreateSequence(ChoreographyFormation formation, int nextFormationIndex)
    {
        if (formation.HasEvent)
        {
            if (formation.Event.Type == ChoreographyEvent.EventType.EarlyRotation)
            {
                RotateSpawnSource(formation.Event.RotationValue);
            }
        }

        var formationHolder = ChoreographyPoolManager.Instance.GetNewFormationHolder();
        if (formationHolder == null)
        {
            throw new NullReferenceException();
        }

        formationHolder.gameObject.SetActive(true);
        var formationTransform = formationHolder.transform;
        formationTransform.SetParent(transform);
        formationTransform.position = _formationStart.position;
        formationTransform.rotation = _formationStart.rotation;

        var tweenSpeed = PlaylistManager.Instance.TargetSpeedMod * SongInfoReader.Instance.NoteSpeed / _meterDistance * 10;

        if (formation.HasNote && PlaylistManager.Instance.ForceJabsOnly)
        {
            var note = formation.Note.SetToBasicJab();
            formation = formation.SetNote(note);
        }
        else if (_lastFormation.HasObstacle && _lastFormation.Obstacle.Type == ChoreographyObstacle.ObstacleType.Dodge)
        {
            if (formation.HasNote && !formation.HasObstacle)
            {
                var targetType = _currentStance == HitSideType.Left ? HitSideType.Right : HitSideType.Left;
                
                var note = formation.Note.SetType(targetType);

                if (note.CutDir == ChoreographyNote.CutDirection.HookLeft && targetType == HitSideType.Left ||
                    note.CutDir == ChoreographyNote.CutDirection.HookRight && targetType == HitSideType.Right)
                {
                    note = note.SetCutDirection(ChoreographyNote.CutDirection.Jab);
                }

                var formations = ChoreographyReader.Instance.GetOrderedFormations();
                if (nextFormationIndex < formations.Count)
                {
                    var nextFormation = formations[nextFormationIndex];
                    if (formations.Count > nextFormationIndex && nextFormation.HasNote && nextFormation.Note.HitSideType == note.HitSideType)
                    {
                        formation = formation.RemoveNote();
                    }
                    else
                    {
                        formation = formation.SetNote(note);
                    }
                }
            }
        }

        var strikePoint = formation.HasNote && formation.Note.IsJab ? _optimalStrikePositioning.JabStrikePoint : _optimalStrikePositioning.DirectionalStrikePoint;

        formationHolder.SetUp(this, formation, nextFormationIndex, strikePoint, _currentRotation);


        var tweenData = new SimpleTween.Data(formationTransform, formationHolder.OnStartCallback,
            formationHolder.OnCompleteCallback, _formationEnd.position, tweenSpeed);
        var tween = ChoreographyPoolManager.Instance.GetNewTween(tweenData);

        if(tween == null)
        {
            return;
        }

        var beatsTime = (60 / SongInfoReader.Instance.BeatsPerMinute) / PlaylistManager.Instance.SongSpeedMod;
        var time = (Time.time - (_songStartTime + _pauseOffset));

        var timeToPoint = (formation.HasNote && formation.Note.IsJab ? _optimalJabPointDistance : _optimalPointDistance) / tweenSpeed;

        var delay = Mathf.Max(0, (formation.Time * beatsTime) - time - timeToPoint);

        tween.DelayTweenStart(delay);

        if (formation.HasEvent)
        {
            if (formation.Event.Type == ChoreographyEvent.EventType.LateRotation)
            {

                RotateSpawnSource(formation.Event.RotationValue);
            }
            else if (formation.Event.Type == ChoreographyEvent.EventType.ChangeFooting)
            {
                WaitAndSwapFooting(delay).Forget();
            }
        }
        _lastFormation = formation;
    }

    private async UniTaskVoid WaitAndSwapFooting(float delay)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: _cancellationToken).SuppressCancellationThrow();
        SwitchFootPlacement();
    }

    public void TryCreateNextSequence(int nextFormationIndex)
    {
        var formations = ChoreographyReader.Instance.GetOrderedFormations();
        if (nextFormationIndex < formations.Count)
        {
            CreateSequence(formations[nextFormationIndex], ++nextFormationIndex);
        }
        else //Sequence is completed
        {
            _sequenceUnstartedOrFinished = true;
            LevelManager.Instance.SetChoreographyCompleted(true);
        }
    }

    public void SpawnFormationObjects(FormationHolder formationHolder, ChoreographyFormation formation)
    {
        if (this?.gameObject == null)
        {
            return;
        }

        if (formation.HasObstacle)
        {
            var obstacle = GetObstacle(formation.Obstacle);
            obstacle.SetUpObstacle();

            var obstacleTransform = obstacle.transform;
            obstacleTransform.SetParent(formationHolder.transform);
            obstacleTransform.localPosition = Vector3.zero;
            obstacleTransform.localRotation = quaternion.identity;

            obstacle.gameObject.SetActive(true);
            ActiveTargetManager.Instance?.AddActiveObstacle(obstacle);

            if (formationHolder.children == null)
            {
                formationHolder.children = new List<IPoolable>();
            }

            formationHolder.Add(obstacle);
        }

        if (formation.HasNote)
        {
            var targetSideType = formation.Note.HitSideType;
            var hasDodgeObstacle = formation.HasObstacle && formation.Obstacle.Type == ChoreographyObstacle.ObstacleType.Dodge;
            var note = formation.Note;
            if (hasDodgeObstacle && note.HitSideType != HitSideType.Block)
            {
                var taregtSide = formation.Obstacle.HitSideType;
                if (_currentStance == HitSideType.Block)
                {
                    targetSideType = taregtSide == HitSideType.Left ? HitSideType.Left : HitSideType.Right;
                }
                else
                {
                    targetSideType = _currentStance;// == HitSideType.Left ? HitSideType.Left : HitSideType.Right;
                }
                note = formation.Note.SetType(targetSideType);
            }

            var target = GetTarget(note);
            target.layer = note.LineLayer;

            var targetTransform = target.transform;
            targetTransform.SetParent(formationHolder.transform);
            targetTransform.localRotation = quaternion.identity;


            targetTransform.localPosition = (GetTargetPosition(note, hasDodgeObstacle));
            var strikeSpeed = note.IsSuperNote ? SettingsManager.GetSuperStrikeHitSpeed(note.HitSideType) : -1;
            target.SetUpTarget(targetSideType, formationHolder.StrikePoint, formationHolder, strikeSpeed);

            target.gameObject.SetActive(true);
            ActiveTargetManager.Instance?.AddActiveTarget(target);
            if (formationHolder.children == null)
            {
                formationHolder.children = new List<IPoolable>();
            }

            formationHolder.Add(target);
        }
    }

    public void TryAddLaneIndicator(float rotation)
    {
        if (_laneIndicators.ContainsKey(rotation))
        {
            _laneIndicators[rotation].AddFormation();
            return;
        }

        var indicator = ChoreographyPoolManager.Instance.GetNewIndicator();
        indicator.SetUp(rotation, _playerCenter);
        indicator.AddFormation();
        _laneIndicators[rotation] = indicator;
    }

    public void TryRemoveLaneIndicator(float rotation)
    {
        if (!_laneIndicators.ContainsKey(rotation))
        {
            return;
        }

        var indicator = _laneIndicators[rotation];
        indicator.RemoveFormation();
        if (indicator.ActiveFormations <= 0)
        {
            _laneIndicators.Remove(rotation);
            indicator.HideAndReturn();
        }
    }

    protected BaseObstacle GetObstacle(ChoreographyObstacle obstacle)
    {
        return ChoreographyPoolManager.Instance.GetObstacle(obstacle, _currentStance);
    }

    protected BaseTarget GetTarget(ChoreographyNote note)
    {
        return ChoreographyPoolManager.Instance.GetTarget(note);
    }

    private Vector3 GetTargetPosition(ChoreographyNote note, bool hasDodgeObstacle)
    {
        var targetSides = TargetSideInfo.GetSetting();
        /*int leftSide;
        int rightSide;
        switch (targetSides)
        {
            case TargetSide.Uncrossed:
                if (!hasDodgeObstacle || (hasDodgeObstacle && _currentStance == HitSideType.Right))
                {
                    leftSide = 3;
                    rightSide = 2;
                }
                else
                {
                    leftSide = 2;
                    rightSide = 3;
                }
                break;
            case TargetSide.Mixed:
                if (note.CutDir is ChoreographyNote.CutDirection.Jab or ChoreographyNote.CutDirection.JabDown or
                    ChoreographyNote.CutDirection.HookLeftDown or ChoreographyNote.CutDirection.HookRightDown)
                {
                    if (!hasDodgeObstacle || (hasDodgeObstacle && _currentStance == HitSideType.Right))
                    {
                        leftSide = 3;
                        rightSide = 2;
                    }
                    else
                    {
                        leftSide = 2;
                        rightSide = 3;
                    }
                }
                else
                {
                    if (!hasDodgeObstacle || (hasDodgeObstacle && _currentStance == HitSideType.Right))
                    {
                        leftSide = 2;
                        rightSide = 3;
                    }
                    else
                    {
                        leftSide = 3;
                        rightSide = 2;
                    }
                }
                break;
            case TargetSide.Crossed:
            default:
                if (!hasDodgeObstacle || (hasDodgeObstacle && _currentStance == HitSideType.Right))
                {
                    leftSide = 2;
                    rightSide = 3;
                }
                else
                {
                    leftSide = 3;
                    rightSide = 2;
                }
                break;
        }*/

        var (leftSide, rightSide) = targetSides switch// wrapping (leftSide, rightSide) together like that causes the switch statement to allow us to assign both variables
        {
            TargetSide.Crossed => (2, 3),
            TargetSide.Uncrossed => !hasDodgeObstacle ? (3, 2) : (2, 3),
            TargetSide.Mixed => (note.CutDir is ChoreographyNote.CutDirection.Jab or ChoreographyNote.CutDirection.JabDown or
                            ChoreographyNote.CutDirection.HookLeftDown or ChoreographyNote.CutDirection.HookRightDown)
                            ? (!hasDodgeObstacle ? (3, 2) : (2, 3)) : (2, 3),
            _ => (2, 3)//!hasDodgeObstacle ? (2, 3) : (3, 2),
        };

        var lineLayer = (int)note.LineLayer * 4;
        switch (note.HitSideType)
        {
            case HitSideType.Block:
                var lineLayerObj =
                    _sequenceStartPoses[Mathf.Min(1 + lineLayer, _sequenceStartPoses.Length - 3)]
                        .localPosition;
                return new Vector3(0, lineLayerObj.y, 0);
            case HitSideType.Left:
                return _sequenceStartPoses[Mathf.Min(4 - leftSide + lineLayer, _sequenceStartPoses.Length - leftSide)]
                    .localPosition;
            case HitSideType.Right:
                return _sequenceStartPoses[Mathf.Min(4 - rightSide + lineLayer, _sequenceStartPoses.Length - rightSide)]
                    .localPosition;
            default:
                return Vector3.zero;
        }
    }

    public void ResetChoreography()
    {
        ChoreographyPoolManager.Instance.CompleteAllActive();

        _formationStart.RotateAround(_playerCenter.position, _playerCenter.up, -_currentRotation);
        _currentRotation = 0;
    }

    public void SequenceRestart()
    {
        _resetting = true;
    }

    public void FinishSequenceRestart()
    {
        _resetting = false;
    }

    public void PauseChoreography()
    {
        _delayStartTime = Time.time;

        SequenceRunning = false;
    }

    public void ResumeChoreography()
    {
        _pauseOffset += Time.time - _delayStartTime;
        SequenceRunning = true;
    }

    public void SwitchFootPlacement()
    {
        if (_currentStance == HitSideType.Block)
        {
            CurrentStance = HitSideType.Block;
            return;
        }
        var stance = _currentStance;
        if (_resetting)
        {
            stance = PlaylistManager.Instance.CurrentPlaylist.StartingSide;
        }
        CurrentStance = stance switch
        {
            HitSideType.Left => HitSideType.Right,
            HitSideType.Right => HitSideType.Left,
            _ => HitSideType.Right
        };
    }

    public void RotateSpawnSource(float angle)
    {
        if (PlaylistManager.Instance.CurrentPlaylist == null)
        {
            Debug.LogError("CurrentPlaylist is null. This is game breaking");
            return;
        }

        var targetGameMode = PlaylistManager.Instance.TargetGameMode;

        if (targetGameMode == GameMode.Degrees90 &&
            Mathf.Abs(_currentRotation + angle) > MAX90ROTATION)
        {
            angle *= -1;
            if (Mathf.Abs(_currentRotation + angle) > MAX90ROTATION)
            {
                angle = angle < 0 ? -MAX90ROTATION : MAX90ROTATION;
            }
        }

        if (targetGameMode != GameMode.Degrees90 ||
            Mathf.Abs(_currentRotation + angle) <= MAX90ROTATION)
        {
            _formationStart.RotateAround(_playerCenter.position, _playerCenter.up, angle);
            _currentRotation += angle;
        }
    }
}