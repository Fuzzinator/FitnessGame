using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddToScore : MonoBehaviour, IValidHit
{
    [SerializeField]
    private int _maxValue = 10;

    [SerializeField]
    private float _hitRange = 2f;
    
    private const int OPTIMALHITMOD = 2;

    public void TriggerHitEffect(HitInfo info)
    {
        var magnitude = info.CollisionData.rigidbody.velocity.magnitude;

        var impactValue = Mathf.Clamp(info.ImpactDotProduct, 0, 1);
        var directionValue = Mathf.Clamp(info.DirectionDotProduct, 0, 1);
        var magnitudeBonusValue = magnitude * .25f * _maxValue;

        var valueAsFloat = impactValue * directionValue * _maxValue + magnitudeBonusValue;

        var hitValue = Mathf.RoundToInt(valueAsFloat * GetOptimalHitModifier(info.DistanceFromOptimalHit));
        
        if (ScoringManager.Instance != null)
        {
            ScoringManager.Instance.AddToScore(hitValue);
        }
    }
    
    private float GetOptimalHitModifier(float distance)
    {
        distance = Mathf.Clamp(distance, 0, _hitRange);
        distance  /= _hitRange;
        return OPTIMALHITMOD - distance;
    }
}