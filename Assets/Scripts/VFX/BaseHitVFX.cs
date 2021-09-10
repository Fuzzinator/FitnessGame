using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BaseHitVFX : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _particleSystem;

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

    public void PlayParticles()
    {
        _particleSystem.Play(true);
        StartCoroutine(WaitParticleAlive());
        //WaitParticleAlive().ToUniTask();
    }

    private async UniTask WaitForParticleFinish()
    {
        await WaitParticleAlive();
        Destroy(gameObject);
    }

    private IEnumerator WaitParticleAlive()
    {
        var frameCountDown = 10;
        while(_particleSystem.IsAlive(true)||frameCountDown>0)
        {
            frameCountDown--;
            yield return null;
        }
        Destroy(gameObject);
    }
}
