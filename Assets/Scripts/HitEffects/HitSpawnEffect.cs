using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class HitSpawnEffect : MonoBehaviour, IValidHit
{
    [SerializeField]
    private Renderer _thisRenderer;

    private void OnValidate()
    {
        if (_thisRenderer == null)
        {
            TryGetComponent(out _thisRenderer);
        }
    }

    public async void TriggerHitEffect(HitInfo info)
    {
        if (VFXManager.Instance == null)
        {
            return;
        }

        var hitParticle = VFXManager.GetBaseHitVFX;
        var hitParticleTransform = hitParticle.transform;
        var thisTransform = transform;

        hitParticleTransform.rotation = thisTransform.rotation;
        hitParticleTransform.position = thisTransform.position;

        hitParticle.SetParticleColor(_thisRenderer.sharedMaterial.color);

        await hitParticle.PlayParticles();
    }
}