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

    private bool _active;
    
    private CancellationToken _token;
    private WaitForSeconds _lifespanSeconds;
    
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
        _token = this.GetCancellationTokenOnDestroy();
        _lifespanSeconds = new WaitForSeconds(_lifespan);
        MonitorParticles().Forget();
    }

    public void SetParticleColor(Color color)
    {
        var main = _particleSystem.main;
        
        main.startColor = color;
    }

    public IEnumerator PlayParticlesCoroutine()
    {
        _particleSystem.Play(true);
        yield return _lifespanSeconds;
        ReturnToPool();
    }
    
    public void PlayParticles()
    {
        _particleSystem.Play(true);
        _active = true;
    }

    public async UniTaskVoid MonitorParticles()
    {
        while (!_token.IsCancellationRequested)
        {
            if (!_active)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(.5f), cancellationToken: _token);
                continue;
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(_lifespan), cancellationToken: _token);
            ReturnToPool();
        }
    }
    
    public void ReturnToPool()
    {
        _active = false;
        _particleSystem.Stop(true);
        transform.SetParent(MyPoolManager.poolParent);
        MyPoolManager.ReturnToPool(this);
    }
}
