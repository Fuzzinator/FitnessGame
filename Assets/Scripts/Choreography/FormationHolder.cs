using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SimpleTweens;
using UnityEngine;

[Serializable]
public class  FormationHolder : MonoBehaviour, IPoolable
{
    private PoolManager _myPoolManager;
    public List<IPoolable> children;
    private bool _isPooled;

    private ChoreographySequencer _sequencer;
    private ChoreographyFormation _formation;
    private int _nextFormationIndex;
    
    public Vector3 StrikePoint { get; private set; }
    
    public Action OnStartCallback { get; private set; }
    public Action OnCompleteCallback { get; private set; }
    
    public PoolManager MyPoolManager
    {
        get => _myPoolManager;
        set => _myPoolManager = value;
    }

    public bool IsPooled
    {
        get => _isPooled;
        set => _isPooled = value;
    }

    public void Initialize()
    {
        SetCallbacks();
    }

    private void SetCallbacks()
    {
        OnStartCallback = OnStart;
        OnCompleteCallback = OnComplete;
    }
    
    private void OnStart()
    {
        _sequencer.SpawnFormationObjects(this, _formation);
        _sequencer.TryCreateNextSequence(_nextFormationIndex);
    }

    private void OnComplete()
    {
        if (this == null)
        {
            return;
        }
        ReturnRemainingChildren();
    }

    public void SetUp(ChoreographySequencer sequencer, ChoreographyFormation formation, int index, Vector3 strikePoint)
    {
        if (OnStartCallback == null)
        {
            SetCallbacks();
        }
        _sequencer = sequencer;
        _formation = formation;
        _nextFormationIndex = index;
        StrikePoint = strikePoint;
    }
    
    public void ReturnRemainingChildren()
    {
        if (children != null)
        {
            while (children.Count > 0)
            {
                var child = children[0];

                if (child != null && !child.IsPooled)
                {
                    if (child is BaseTarget target)
                    {
                        target.Complete();
                    }
                    else
                    {
                        child.ReturnToPool();
                    }
                }

                children.Remove(child);
            }
        }

        ReturnToPool();
    }

    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        transform.SetParent(MyPoolManager.poolParent);
        ((IPoolable) this).MyPoolManager.ReturnToPool(this);
        IsPooled = true;
    }

    public void Add(IPoolable poolable)
    {
        if (children != null)
        {
            children.Add(poolable);
        }
    }
    public void Remove(IPoolable poolable)
    {
        if (children != null)
        {
            children.Remove(poolable);
        }
    }
}