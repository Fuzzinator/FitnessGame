using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

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

    private void Start()
    {
        _normalHitPool = new PoolManager(_normalHitPrefab, transform);
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