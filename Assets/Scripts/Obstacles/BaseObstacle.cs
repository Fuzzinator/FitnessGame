using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseObstacle : MonoBehaviour, IPoolable
{
    
    public PoolManager MyPoolManager { get; set; }

    public bool IsPooled { get; set; }
    
    [SerializeField]
    protected UnityEvent _hitHeadEvent = new UnityEvent();

    protected void OnTriggerEnter(Collider other)
    {
        if (!IsHit(other))
        {
            return;
        }
        _hitHeadEvent?.Invoke();
    }

    protected bool IsHit(Collider col)
    {
        return col.gameObject.layer == LayerMask.NameToLayer("Hand");
    }
    
    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        transform.SetParent(MyPoolManager.poolParent);
        MyPoolManager.ReturnToPool(this);
    }
}
