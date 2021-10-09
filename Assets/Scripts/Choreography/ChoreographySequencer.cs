using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ChoreographySequencer : MonoBehaviour
{
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
    private Transform _formationStart;

    [SerializeField]
    private Transform _formationEnd;


    private Sequence _sequence;

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


    private List<Sequence> _activeSequences = new List<Sequence>();

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

    public bool test = false;


    private Action<InputAction.CallbackContext> _selectAction;

    [SerializeField]
    private UnityEvent _sequenceStarted = new UnityEvent();

    [SerializeField]
    private UnityEvent<int> _stanceUpdated = new UnityEvent<int>();

    
    private Vector3[] _path;
    private float _tweenSpeed;

    public bool SequenceRunning { get; private set; }

    #region Const Strings

    private const string SELECT = "Select";
    private const string MENUBUTTON = "Menu Button";
#if UNITY_EDITOR
    private const string PAUSEINEDITOR = "Pause In Editor";
#endif

    #endregion

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

        _meterDistance = Vector3.Distance(_formationStart.position, _formationEnd.position);
        _optimalPointDistance = Vector3.Distance(_formationStart.position, _optimalStrikePoint.position);

        _selectAction = (context) => TempStart();

        _path = new[]
                {
                    _formationStart.position,
                    _formationEnd.position
                };
        
        _tweenSpeed = _meterDistance / SongInfoReader.Instance.NoteSpeed;
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null && InputManager.Instance.MainInput != null)
        {
            if (InputManager.Instance != null && InputManager.Instance.MainInput != null)
            {
                InputManager.Instance.MainInput[SELECT].performed += _selectAction;
                InputManager.Instance.MainInput[MENUBUTTON].performed += ToggleChoreography;
                FocusTracker.Instance.focusChanged.AddListener(ToggleChoreography);
#if UNITY_EDITOR
                InputManager.Instance.MainInput[PAUSEINEDITOR].performed += ToggleChoreography;
#endif
            }
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null && InputManager.Instance.MainInput != null)
        {
            if (InputManager.Instance != null && InputManager.Instance.MainInput != null)
            {
                InputManager.Instance.MainInput[SELECT].performed -= _selectAction;
                InputManager.Instance.MainInput[MENUBUTTON].performed -= ToggleChoreography;
                FocusTracker.Instance.focusChanged.RemoveListener(ToggleChoreography);
#if UNITY_EDITOR
                InputManager.Instance.MainInput[PAUSEINEDITOR].performed -= ToggleChoreography;
#endif
            }
        }
    }

    public void TempStart()
    {
        if (SequenceRunning)
        {
            return;
        }

        InitializeSequence();
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
        DOTween.Init(true, false);
        _sequence = DOTween.Sequence();

        var formationSequence = CreateSequence(formations[0], 1);
        _activeSequences.Add(formationSequence);
        SequenceRunning = true;
    }

    private Sequence CreateSequence(ChoreographyFormation formation, int nextFormationIndex)
    {
        var sequence = DOTween.Sequence();

        var formationHolder = _formationHolderPool.GetNewPoolable() as FormationHolder;
        formationHolder.gameObject.SetActive(true);
        var formationTransform = formationHolder.transform;
        formationTransform.SetParent(transform);
        formationTransform.position = _formationStart.position;

        var tween = formationHolder.transform.DOLocalPath(_path, _tweenSpeed);

        tween.SetEase(Ease.Linear);

        TweenCallback onStart = () => SpawnFormationObjects(formationHolder, formation);
        onStart += () => TryCreateNextSequence(nextFormationIndex);
        //onStart += () => _activeSequences.Add(sequence);

        TweenCallback onComplete = () => ClearFormationObjects(formationHolder);
        onComplete += () => _activeSequences.Remove(sequence);
        tween.OnStart(onStart);

        tween.OnComplete(onComplete);


        var beatsTime = 60 / SongInfoReader.Instance.BeatsPerMinute;
        var time = (Time.time - (_songStartTime + _pauseOffset));
        var timeToPoint = _optimalPointDistance / SongInfoReader.Instance.NoteSpeed;

        var delay = Mathf.Max(0, (formation.Time * beatsTime) - time - timeToPoint);

        sequence.Insert(delay, tween);

        return sequence;
    }

    private void TryCreateNextSequence(int nextFormationIndex)
    {
        var formations = ChoreographyReader.Instance.GetOrderedFormations();
        if (nextFormationIndex < formations.Count)
        {
            var formationSequence = CreateSequence(formations[nextFormationIndex], ++nextFormationIndex);

            _activeSequences.Add(formationSequence);
            //_sequence.Insert(formations[nextFormationIndex].Time, tween);
        }
    }

    private void SpawnFormationObjects(FormationHolder formationHolder, ChoreographyFormation formation)
    {
        if (formation.HasObstacle)
        {
            var obstacle = GetObstacle(formation.Obstacle);
            obstacle.transform.SetParent(formationHolder.transform);
            obstacle.transform.localPosition = Vector3.zero;
            obstacle.gameObject.SetActive(true);

            if (formationHolder.children == null)
            {
                formationHolder.children = new List<IPoolable>();
            }

            formationHolder.children.Add(obstacle);
        }

        if (formation.HasNote)
        {
            var target = GetTarget(formation.Note);
            target.SetUpTarget(formation.Note.Type);
            target.transform.SetParent(formationHolder.transform);
            target.transform.position = (GetTargetParent(formation.Note)).position;
            target.gameObject.SetActive(true);
            ActiveTargetManager.Instance.AddActiveTarget(target);
            if (formationHolder.children == null)
            {
                formationHolder.children = new List<IPoolable>();
            }

            formationHolder.children.Add(target);
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

    private Transform GetTargetParent(ChoreographyNote note)
    {
        switch (note.HitSideType)
        {
            case HitSideType.Block:
            case HitSideType.Left:
                return _sequenceStartPoses[(1 + (int) note.LineLayer * 4)];
            case HitSideType.Right:
                return _sequenceStartPoses[(2 + (int) note.LineLayer * 4)];
            default:
                return null;
        }
    }

    public void ToggleChoreography(InputAction.CallbackContext context)
    {
        if (SequenceRunning)
        {
            PauseChoreography();
        }
        else
        {
            ResumeChoreography();
        }
    }

    private void ToggleChoreography(bool play)
    {
        if (play)
        {
            ResumeChoreography();
        }
        else
        {
            PauseChoreography();
        }
    }

    public void PauseChoreography()
    {
        foreach (var sequence in _activeSequences)
        {
            sequence.Pause();
        }


        _delayStartTime = Time.time;

        SequenceRunning = false;
    }

    public void ResumeChoreography()
    {
        foreach (var sequence in _activeSequences)
        {
            sequence.Play();
        }

        _pauseOffset += Time.time - _delayStartTime;
        SequenceRunning = true;
    }

    private void ClearFormationObjects(FormationHolder formationHolder)
    {
        formationHolder.ReturnRemainingChildren();
    }

    public void SwitchFootPlacement()
    {
        CurrentStance = _currentStance == HitSideType.Left ? HitSideType.Right : HitSideType.Left;
    }
}