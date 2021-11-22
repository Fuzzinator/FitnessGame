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

    private CancellationToken token;

    private const int FRAMEDELAY = 5;
    
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
        await UniTask.DelayFrame(FRAMEDELAY, cancellationToken: token);
        await WaitForParticleFinish();
    }

    private async UniTask WaitForParticleFinish()
    {
        var timeSpan = TimeSpan.FromSeconds(.25f);
        while (_particleSystem.IsAlive(true))
        {
            await UniTask.Delay(timeSpan, cancellationToken: token);
        }
        
        if (this == null)
        {
            return;
        }
        Destroy(gameObject);
    }
    
    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        transform.SetParent(MyPoolManager.poolParent);
        MyPoolManager.ReturnToPool(this);
    }
}
