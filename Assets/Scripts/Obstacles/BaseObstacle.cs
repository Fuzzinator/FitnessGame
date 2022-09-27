using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseObstacle : MonoBehaviour, IPoolable
{
    [SerializeField]
    private SetRendererMaterial _setRendererMaterial;
    public PoolManager MyPoolManager { get; set; }

    public bool IsPooled { get; set; }

    public void Initialize()
    {
        _setRendererMaterial.Initialize(HitSideType.Unused);
    }
    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        if (MyPoolManager.poolParent != null)
        {
            transform.SetParent(MyPoolManager.poolParent);
        }

        MyPoolManager.ReturnToPool(this);
    }
}
