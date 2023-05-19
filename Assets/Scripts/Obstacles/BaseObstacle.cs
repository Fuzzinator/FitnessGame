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

    [SerializeField]
    private Collider[] _colliders;

    public PoolManager MyPoolManager { get; set; }

    public SetRendererMaterial RendererSetter => _setRendererMaterial;
    public Collider[] Colliders => _colliders;
    public bool IsPooled { get; set; }
    public bool WasHit { get; private set; }

    private void OnValidate()
    {
        if(_colliders != null && _colliders.Length != 0)
        {
            return;
        }

        _colliders = GetComponentsInChildren<Collider>();
    }

    public void Initialize()
    {
        _setRendererMaterial.Initialize(HitSideType.Unused, false);
        _setRendererColor?.Initialize();
    }
    public void ReturnToPool()
    {
        if(!WasHit)
        {
            ScoringAndHitStatsManager.Instance.RegisterDodgedObstacle();
        }

        gameObject.SetActive(false);
        if (MyPoolManager.poolParent != null)
        {
            transform.SetParent(MyPoolManager.poolParent);
        }
        ActiveTargetManager.Instance.RemoveActiveObstacle(this);
        MyPoolManager.ReturnToPool(this);
    }

    public virtual void SetUpObstacle()
    {
        WasHit = false;
    }

    public void RegisterHit()
    {
        WasHit = true;        
    }
}
