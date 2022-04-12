using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameModeManagement;
using SimpleTweens;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ChoreographySequencer : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField]
    private FormationHolder _formationHolderPrefab;

    private PoolManager _formationHolderPool;

    [SerializeField]
    private BaseTarget _jabTarget;

    private PoolManager _jabPool;

    [SerializeField]
    private BaseTarget _leftHookTarget;

    private PoolManager _leftHookPool;

    [SerializeField]
    private BaseTarget _rightHookTarget;

    private PoolManager _rightHookPool;

    [SerializeField]
    private BaseTarget _uppercutTarget;

    private PoolManager _uppercutPool;

    [SerializeField]
    private BlockTarget _baseBlockTarget;

    private PoolManager _baseBlockPool;

    [Header("Obstacles")]
    [SerializeField]
    private BaseObstacle _baseObstacle;

    private PoolManager _baseObstaclePool;

    [SerializeField]
    private BaseObstacle _leftObstacle;

    private PoolManager _leftObstaclePool;

    [SerializeField]
    private BaseObstacle _rightObstacle;

    private PoolManager _rightObstaclePool;

    [SerializeField]
    private ActiveLaneIndicator _laneIndicator;

    private PoolManager _laneIndicatorPool;

    private SimpleTweenPool _tweenPool;

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
    private Transform _optimalStrikePoint;

    private float _meterDistance;
    private float _optimalPointDistance;
    private float _songStartTime;
    private float _delayStartTime;
    private float _pauseOffset;

    private float _currentRotation = 0;
    private const float MAX90ROTATION = 45;

    private CancellationToken _cancellationToken;

    [SerializeField]
    private HitSideType _currentStance = HitSideType.Left;

    public HitSideType CurrentStance
    {
        get => _currentStance;
        set
        {
            _currentStance = value;
            var temp = value;
            temp++;
            _stanceUpdated?.Invoke((int) temp);
        }
    }

    [Header("Events")]
    [SerializeField]
    private UnityEvent _sequenceStarted = new UnityEvent();

    [SerializeField]
    private UnityEvent<int> _stanceUpdated = new UnityEvent<int>();

    private bool _sequenceUnstartedOrFinished = true;
    
    private Dictionary<float, ActiveLaneIndicator> _laneIndicators = new Dictionary<float, ActiveLaneIndicator>(20);
    public bool SequenceRunning { get; private set; }
    private BaseTarget GetTargetSwitch(ChoreographyNote.CutDirection cutDirection) => cutDirection switch
    {
        ChoreographyNote.CutDirection.Jab => _jabPool.GetNewPoolable(),
        ChoreographyNote.CutDirection.JabDown => _jabPool.GetNewPoolable(),
        ChoreographyNote.CutDirection.HookLeft => _leftHookPool.GetNewPoolable(),
        ChoreographyNote.CutDirection.HookLeftDown => _jabPool.GetNewPoolable(),
        ChoreographyNote.CutDirection.HookRight => _rightHookPool.GetNewPoolable(),
        ChoreographyNote.CutDirection.HookRightDown => _jabPool.GetNewPoolable(),
        ChoreographyNote.CutDirection.Uppercut => _uppercutPool.GetNewPoolable(),
        ChoreographyNote.CutDirection.UppercutLeft => _uppercutPool.GetNewPoolable(),
        ChoreographyNote.CutDirection.UppercutRight => _uppercutPool.GetNewPoolable(),
        _ => null,
    } as BaseTarget;

    // Start is called before the first frame update
    void Start()
    {
        var thisTransform = transform;
        _formationHolderPool = new PoolManager(_formationHolderPrefab, thisTransform);
        _jabPool = new PoolManager(_jabTarget, thisTransform);
        _leftHookPool = new PoolManager(_leftHookTarget, thisTransform);
        _rightHookPool = new PoolManager(_rightHookTarget, thisTransform);
        _uppercutPool = new PoolManager(_uppercutTarget, thisTransform);
        _baseBlockPool = new PoolManager(_baseBlockTarget, thisTransform);

        _baseObstaclePool = new PoolManager(_baseObstacle, thisTransform);
        _leftObstaclePool = new PoolManager(_leftObstacle, thisTransform);
        _rightObstaclePool = new PoolManager(_rightObstacle, thisTransform);

        _laneIndicatorPool = new PoolManager(_laneIndicator, thisTransform);

        _cancellationToken = this.GetCancellationTokenOnDestroy();
        _tweenPool = new SimpleTweenPool(20, _cancellationToken);

        var position = _formationStart.position;
        _meterDistance = Vector3.Distance(position, _formationEnd.position);
        _optimalPointDistance = Vector3.Distance(position, _optimalStrikePoint.position);

        _sequenceUnstartedOrFinished = true;
    }

    private void OnEnable()
    {
        GameStateManager.Instance.gameStateChanged.AddListener(GameStateListener);
    }

    private void OnDisable()
    {
        GameStateManager.Instance.gameStateChanged.RemoveListener(GameStateListener);
    }

    private void OnDestroy()
    {
        _tweenPool.CompleteAllActive();
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

    public void InitializeSequence()
    {
        if (ChoreographyReader.Instance == null)
        {
            Debug.LogError("No ChoreographyReader");
            return;
        }

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
        if (formation.HasEvent && formation.Event.Type == ChoreographyEvent.EventType.EarlyRotation)
        {
            RotateSpawnSource(formation.Event.RotationValue);
        }

        var formationHolder = _formationHolderPool.GetNewPoolable() as FormationHolder;
        if (formationHolder == null)
        {
            throw new NullReferenceException();
        }

        formationHolder.gameObject.SetActive(true);
        var formationTransform = formationHolder.transform;
        formationTransform.SetParent(transform);
        formationTransform.position = _formationStart.position;
        formationTransform.rotation = _formationStart.rotation;

        var tweenSpeed = _meterDistance * 10 / SongInfoReader.Instance.NoteSpeed;


        formationHolder.SetUp(this, formation, nextFormationIndex, _optimalStrikePoint.position, _currentRotation);


        var tweenData = new SimpleTween.Data(formationTransform, formationHolder.OnStartCallback,
            formationHolder.OnCompleteCallback, _formationEnd.position, tweenSpeed);
        var tween = _tweenPool.GetNewTween(tweenData);


        var beatsTime = 60 / SongInfoReader.Instance.BeatsPerMinute;
        var time = (Time.time - (_songStartTime + _pauseOffset));
        var timeToPoint = _optimalPointDistance / SongInfoReader.Instance.NoteSpeed;

        var delay = Mathf.Max(0, (formation.Time * beatsTime) - time - timeToPoint);

        tween.DelayTweenStart(delay);

        if (formation.HasEvent && formation.Event.Type == ChoreographyEvent.EventType.LateRotation)
        {
            RotateSpawnSource(formation.Event.RotationValue);
        }
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

            var obstacleTransform = obstacle.transform;
            obstacleTransform.SetParent(formationHolder.transform);
            obstacleTransform.localPosition = Vector3.zero;
            obstacleTransform.localRotation = quaternion.identity;

            obstacle.gameObject.SetActive(true);

            if (formationHolder.children == null)
            {
                formationHolder.children = new List<IPoolable>();
            }

            formationHolder.Add(obstacle);
        }

        if (formation.HasNote)
        {
            var target = GetTarget(formation.Note);
            target.SetUpTarget(formation.Note.Type, formationHolder.StrikePoint, formationHolder);
            target.layer = formation.Note.LineLayer;

            var targetTransform = target.transform;
            targetTransform.SetParent(formationHolder.transform);
            targetTransform.localRotation = quaternion.identity;
            targetTransform.localPosition = (GetTargetPosition(formation.Note));

            target.gameObject.SetActive(true);
            ActiveTargetManager.Instance.AddActiveTarget(target);
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

        var indicator = _laneIndicatorPool.GetNewPoolable() as ActiveLaneIndicator;
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
        return obstacle.Type switch
        {
            ChoreographyObstacle.ObstacleType.Crouch => _baseObstaclePool.GetNewPoolable(),
            ChoreographyObstacle.ObstacleType.Dodge => _currentStance == HitSideType.Left
                ? _leftObstaclePool.GetNewPoolable()
                : _rightObstaclePool.GetNewPoolable(),
            _ => _baseObstaclePool.GetNewPoolable()
        } as BaseObstacle;
    }

    protected BaseTarget GetTarget(ChoreographyNote note)
    {
        if (note.HitSideType == HitSideType.Block)
        {
            return _baseBlockPool.GetNewPoolable() as BaseTarget;
        }
        else
        {
            return GetTargetSwitch(note.CutDir);
        }
    }

    private Vector3 GetTargetPosition(ChoreographyNote note)
    {
        switch (note.HitSideType)
        {
            case HitSideType.Block:
                var lineLayerObj =
                    _sequenceStartPoses[Mathf.Min(1 + (int) note.LineLayer * 4, _sequenceStartPoses.Length - 3)]
                        .localPosition;
                return new Vector3(0, lineLayerObj.y, 0);
            case HitSideType.Left:
                return _sequenceStartPoses[Mathf.Min(1 + (int) note.LineLayer * 4, _sequenceStartPoses.Length - 3)]
                    .localPosition;
            case HitSideType.Right:
                return _sequenceStartPoses[Mathf.Min(2 + (int) note.LineLayer * 4, _sequenceStartPoses.Length - 2)]
                    .localPosition;
            default:
                return Vector3.zero;
        }
    }

    public void ResetChoreography()
    {
        _tweenPool.CompleteAllActive();

        _formationStart.RotateAround(_playerCenter.position, _playerCenter.up, -_currentRotation);
        _currentRotation = 0;
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
        CurrentStance = _currentStance == HitSideType.Left ? HitSideType.Right : HitSideType.Left;
    }

    public void RotateSpawnSource(float angle)
    {
        var targetGameMode = PlaylistManager.Instance.OverrideGameMode
            ? GameManager.Instance.CurrentGameMode
            : PlaylistManager.Instance.CurrentItem.TargetGameMode;

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