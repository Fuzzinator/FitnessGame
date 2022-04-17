using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BaseHitVFX : MonoBehaviour, IPoolable
{
    [SerializeField]
    private ParticleSystem _particleSystem;
    
    public PoolManager MyPoolManager { get; set; }

    public bool IsPooled { get; set; }
    private void OnValidate()
    {
        if (_particleSystem == null)
        {
            TryGetComponent(out _particleSystem);
        }
    }

    public void Initialize()
    {
    }

    public void SetParticleColor(Color color)
    {
        var main = _particleSystem.main;
        
        main.startColor = color;
    }
    
    public void PlayParticles()
    {
        gameObject.SetActive(true);
        _particleSystem.Play(true);
    }

    private void OnParticleSystemStopped()
    {
        ReturnToPool();
    }
    
    public void ReturnToPool()
    {
        _particleSystem.Stop(true);
        gameObject.SetActive(false);
        MyPoolManager.ReturnToPool(this);
    }
}
