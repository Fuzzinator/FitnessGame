using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class FormationHolder : MonoBehaviour, IPoolable
{
    private PoolManager _myPoolManager;
    public List<IPoolable> children;
    private bool _isPooled;

    [SerializeField]
    private bool hasNote;
    [SerializeField]
    private bool hasObstacle;
    
    public Tween MyTween { get; set; }

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
    
    public void ReturnRemainingChildren()
    {
        if (children != null)
        {
            while (children.Count > 0)
            {
                var child = children[0];

                if (child != null && !child.IsPooled)
                {
                    child.ReturnToPool();
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
        hasNote = false;
        hasObstacle = false;
        IsPooled = true;
    }

    public void Add(IPoolable poolable)
    {
        if (children != null)
        {
            children.Add(poolable);
            
            switch (poolable)
            {
                case BaseTarget note:
                    hasNote = true;
                    break;
                case BaseObstacle obstacle:
                    hasObstacle = true;
                    break;
            }
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