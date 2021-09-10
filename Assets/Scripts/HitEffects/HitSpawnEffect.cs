using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class HitSpawnEffect : MonoBehaviour, IValidHit
{
    [SerializeField]
    private BaseHitVFX _hitVFX;
    [SerializeField]
    private Renderer _thisRenderer;

    private void OnValidate()
    {
        if (_thisRenderer == null)
        {
            TryGetComponent(out _thisRenderer);
        }
    }

    public void TriggerHitEffect(HitInfo info)
    {
        var hitParticle = Instantiate(_hitVFX, null);
        var hitParticleTransform = hitParticle.transform;
        var thisTransform = transform;
        
        hitParticleTransform.rotation = thisTransform.rotation;
        hitParticleTransform.position = thisTransform.position;
        
        hitParticle.SetParticleColor(_thisRenderer.sharedMaterial.color);
        hitParticle.PlayParticles();
    }
}
