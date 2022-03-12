using System;
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

    public void TriggerHitEffect(HitInfo info)
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
        hitParticle.StartCoroutine(hitParticle.PlayParticlesCoroutine());
        
        /*try
        {
            hitParticle.PlayParticles().Forget();
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
            return;
        }*/
    }
}