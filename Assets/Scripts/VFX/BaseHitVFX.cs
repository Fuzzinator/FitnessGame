using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BaseHitVFX : MonoBehaviour, IPoolable
{
    [SerializeField]
    private ParticleSystem[] _particleSystems;

    public PoolManager MyPoolManager { get; set; }

    public bool IsPooled { get; set; }

    private void OnValidate()
    {
        if (_particleSystems == null)
        {
            
            TryGetComponent(out ParticleSystem system);
            _particleSystems = new[] {system};
        }
    }

    public void Initialize()
    {
    }

    public void SetParticleColor(Color color)
    {
        foreach (var ps in _particleSystems)
        {
            var main = ps.main;

            main.startColor = color;
        }
    }

    public void PlayParticles()
    {
        gameObject.SetActive(true);

        foreach (var ps in _particleSystems)
        {
            ps.Play(true);
        }
    }

    private void OnParticleSystemStopped()
    {
        ReturnToPool();
    }

    public void ReturnToPool()
    {
        foreach (var ps in _particleSystems)
        {
            ps.Stop(true);
        }

        gameObject.SetActive(false);
        MyPoolManager.ReturnToPool(this);
    }
}