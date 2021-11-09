using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseObstacle : MonoBehaviour, IPoolable
{
    
    public PoolManager MyPoolManager { get; set; }

    public bool IsPooled { get; set; }
    
    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        transform.SetParent(MyPoolManager.poolParent);
        MyPoolManager.ReturnToPool(this);
    }
}
