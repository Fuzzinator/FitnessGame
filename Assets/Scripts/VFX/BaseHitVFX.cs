using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BaseHitVFX : MonoBehaviour, IPoolable
{
    [SerializeField]
    private ParticleSystem _particleSystem;

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

    public void SetParticleColor(Color color)
    {
        var main = _particleSystem.main;
        
        main.startColor = color;
    }

    public async UniTask PlayParticles()
    {
        _particleSystem.Play(true);
        
        await UniTask.DelayFrame(FRAMEDELAY);
        WaitForParticleFinish();
    }

    private async UniTask WaitForParticleFinish()
    {
        await UniTask.WaitWhile(()=>_particleSystem.IsAlive(true));
        Destroy(gameObject);
    }
    
    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        transform.SetParent(MyPoolManager.poolParent);
        MyPoolManager.ReturnToPool(this);
    }

}
