using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseObstacle : MonoBehaviour, IPoolable
{
    [SerializeField]
    private SetRendererMaterial _setRendererMaterial;

    [SerializeField]
    private SetRendererColor _setRendererColor;
    public PoolManager MyPoolManager { get; set; }

    public SetRendererMaterial RendererSetter => _setRendererMaterial;
    public bool IsPooled { get; set; }

    public void Initialize()
    {
        _setRendererMaterial.Initialize(HitSideType.Unused, false);
        _setRendererColor?.Initialize();
    }
    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        if (MyPoolManager.poolParent != null)
        {
            transform.SetParent(MyPoolManager.poolParent);
        }
        ActiveTargetManager.Instance.RemoveActiveObstacle(this);
        MyPoolManager.ReturnToPool(this);
    }
}
