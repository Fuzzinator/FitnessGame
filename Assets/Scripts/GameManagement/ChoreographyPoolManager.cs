using Cysharp.Threading.Tasks;
using SimpleTweens;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ChoreographyPoolManager : BaseGameStateListener
{
    public static ChoreographyPoolManager Instance { get; private set; }

    [SerializeField]
    private ActiveLaneIndicator _laneIndicator;

    [SerializeField]
    private FormationHolder _formationHolderPrefab;
    private PoolManager _formationHolderPool;

    private BaseTarget _jabTarget;
    private PoolManager _jabPool;

    private BaseTarget _leftHookTarget;
    private PoolManager _leftHookPool;

    private BaseTarget _rightHookTarget;
    private PoolManager _rightHookPool;

    private BaseTarget _uppercutTarget;
    private PoolManager _uppercutPool;

    private BlockTarget _baseBlockTarget;
    private PoolManager _baseBlockPool;

    private BaseObstacle _baseObstacle;
    private PoolManager _baseObstaclePool;

    private BaseObstacle _leftObstacle;
    private PoolManager _leftObstaclePool;

    private BaseObstacle _rightObstacle;
    private PoolManager _rightObstaclePool;


    private PoolManager _laneIndicatorPool;

    private SimpleTweenPool _tweenPool;

    private CancellationToken _cancellationToken;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    public void InitializePools()
    {
        if (EnvironmentControlManager.Instance != null)
        {
            UpdateTargetsAndObstacles();
        }

        InitializePoolsAsync().Forget();
    }

    private void UpdateTargetsAndObstacles()
    {
        var assets = EnvironmentControlManager.Instance.ActiveEnvironmentContainer;
        _jabTarget = assets.JabTarget;
        _leftHookTarget = assets.HookLeftTarget;
        _rightHookTarget = assets.HookRightTarget;
        _uppercutTarget = assets.UppercutTarget;
        _baseBlockTarget = assets.BlockTarget;
        _baseObstacle = assets.DuckObstacle;
        _leftObstacle = assets.DodgeLeftObstacle;
        _rightObstacle = assets.DodgeRightObstacle;
    }

    private async UniTaskVoid InitializePoolsAsync()
    {
        var thisTransform = transform;

        _formationHolderPool = new PoolManager(_formationHolderPrefab, thisTransform);
        await UniTask.NextFrame();
        _jabPool = new PoolManager(_jabTarget, thisTransform);
        await UniTask.NextFrame();
        _leftHookPool = new PoolManager(_leftHookTarget, thisTransform);
        await UniTask.NextFrame();
        _rightHookPool = new PoolManager(_rightHookTarget, thisTransform);
        await UniTask.NextFrame();
        _uppercutPool = new PoolManager(_uppercutTarget, thisTransform);
        await UniTask.NextFrame();
        _baseBlockPool = new PoolManager(_baseBlockTarget, thisTransform);
        await UniTask.NextFrame();

        _baseObstaclePool = new PoolManager(_baseObstacle, thisTransform);
        await UniTask.NextFrame();
        _leftObstaclePool = new PoolManager(_leftObstacle, thisTransform);
        await UniTask.NextFrame();
        _rightObstaclePool = new PoolManager(_rightObstacle, thisTransform);
        await UniTask.NextFrame();

        _laneIndicatorPool = new PoolManager(_laneIndicator, thisTransform);
        await UniTask.NextFrame();

        _tweenPool = new SimpleTweenPool(20, _cancellationToken);
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

    public BaseTarget GetTarget(ChoreographyNote note)
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

    public BaseObstacle GetObstacle(ChoreographyObstacle obstacle, HitSideType currentStance)
    {
        if (obstacle.Type == ChoreographyObstacle.ObstacleType.Crouch)
        {
            return _baseObstaclePool.GetNewPoolable() as BaseObstacle;
        }
        else if (obstacle.Type == ChoreographyObstacle.ObstacleType.Dodge)
        {
            if (currentStance == HitSideType.Block)
            {
                return obstacle.HitSideType switch
                {
                    HitSideType.Left => _leftObstaclePool.GetNewPoolable(),
                    HitSideType.Right => _rightObstaclePool.GetNewPoolable(),
                    _ => _baseObstaclePool.GetNewPoolable()
                } as BaseObstacle;
            }
            else
            {
                return (currentStance == HitSideType.Left ?
                    _leftObstaclePool.GetNewPoolable() :
                    _rightObstaclePool.GetNewPoolable()) as BaseObstacle;
            }
        }
        else
        {
            return _baseObstaclePool.GetNewPoolable() as BaseObstacle;
        }
    }

    public FormationHolder GetNewFormationHolder()
    {
        return _formationHolderPool.GetNewPoolable() as FormationHolder;
    }

    public SimpleTween GetNewTween(SimpleTween.Data tweenData)
    {
        return _tweenPool?.GetNewTween(tweenData);
    }

    public ActiveLaneIndicator GetNewIndicator()
    {
        return _laneIndicatorPool.GetNewPoolable() as ActiveLaneIndicator;
    }

    public void CompleteAllActive()
    {
        _tweenPool?.CompleteAllActive();
    }

    protected override void GameStateListener(GameState oldState, GameState newState)
    {
        if(newState == GameState.InMainMenu && oldState != GameState.Paused && oldState != GameState.Unfocused && oldState != GameState.Entry)
        {
            CleanUp();
        }
    }

    private void CleanUp()
    {
        if(_tweenPool == null)
        {
            return;
        }

        _tweenPool.CompleteAllActive();
        _formationHolderPool.CleanUp();
        _jabPool.CleanUp();
        _leftHookPool.CleanUp();
        _rightHookPool.CleanUp();
        _uppercutPool.CleanUp();
        _baseBlockPool.CleanUp();
        _baseObstaclePool.CleanUp();
        _leftObstaclePool.CleanUp();
        _rightObstaclePool.CleanUp();
        _laneIndicatorPool.CleanUp();
        _tweenPool = null;
    }
}
