using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class VFXManager : MonoBehaviour, IOrderedInitialize
{
    public static VFXManager Instance { get; private set; }
    public bool Initialized { get; private set; }


    [SerializeField]
    private BaseHitVFX _normalHitPrefab;

    private PoolManager _normalHitPool;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void Initialize()
    {
        if(Initialized)
        {
            return;
        }

        if (EnvironmentControlManager.Instance != null)
        {
            _normalHitPrefab = EnvironmentControlManager.Instance.ActiveEnvironmentContainer.BaseHitVFX;
        }
        _normalHitPool = new PoolManager(_normalHitPrefab, transform, 20);
        Initialized = true;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static void SetNormalHitPrefab(BaseHitVFX hitVFX)
    {
        Instance._normalHitPrefab = hitVFX;
    }

    public static BaseHitVFX GetBaseHitVFX
    {
        get
        {
            var vfx = Instance._normalHitPool.GetNewPoolable() as BaseHitVFX;
            vfx.MyPoolManager = Instance._normalHitPool;
            return vfx;
        }
    }
}