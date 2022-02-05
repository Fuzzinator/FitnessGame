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

    [SerializeField]
    private float _lifespan = 1.25f;
    
    private CancellationToken token;
    
    public PoolManager MyPoolManager { get; set; }

    public bool IsPooled { get; set; }
    private void OnValidate()
    {
        if (_particleSystem == null)
        {
            TryGetComponent(out _particleSystem);
        }
    }

    private void Start()
    {
        token = this.GetCancellationTokenOnDestroy();
    }

    public void SetParticleColor(Color color)
    {
        var main = _particleSystem.main;
        
        main.startColor = color;
    }

    public async UniTask PlayParticles()
    {
        _particleSystem.Play(true);
        await UniTask.Delay(TimeSpan.FromSeconds(_lifespan), cancellationToken: token);
        ReturnToPool();
    }
    
    public void ReturnToPool()
    {
        _particleSystem.Stop(true);
        transform.SetParent(MyPoolManager.poolParent);
        MyPoolManager.ReturnToPool(this);
    }
}
