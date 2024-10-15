using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class BaseObstacle : MonoBehaviour, IPoolable
{
    [SerializeField]
    private SetRendererMaterial _setRendererMaterial;
/*
    [SerializeField]
    private SetRendererColor _setRendererColor;*/

    [SerializeField]
    private IInitializer[] _initializers = null;

    /*[SerializeField]
    private ObjectInitializer[] _initializers;*/

    [SerializeField]
    private Collider[] _colliders;

    public PoolManager MyPoolManager { get; set; }

    public SetRendererMaterial RendererSetter
    {
        get
        {
            if(_setRendererMaterial == null)
            {
                if(_initializers != null && _initializers.Length>0)
                {
                    foreach(SetRendererMaterial init in _initializers)
                    {
                        if(init != null)
                        {
                            _setRendererMaterial = init;
                            break;
                        }
                    }
                }
            }

            return _setRendererMaterial;
        }
    }
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
        _initializers ??= GetComponents<IInitializer>();
        if(_initializers == null ||  _initializers.Length == 0 )
        {
            return;
        }

        foreach(var initializer in _initializers)
        {
            initializer.Initialize(this);
        }
    }
    public void ReturnToPool()
    {
        if(!WasHit)
        {
            ScoringAndHitStatsManager.Instance?.RegisterDodgedObstacle();
        }

        gameObject.SetActive(false);
        if (MyPoolManager.poolParent != null)
        {
            transform.SetParent(MyPoolManager.poolParent);
        }
        ActiveTargetManager.Instance?.RemoveActiveObstacle(this);
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

#if UNITY_EDITOR
public class BaseObstacleEditor  : EditorSetGlobalTextures
{

}
#endif