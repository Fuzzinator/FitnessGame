using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddToScore : MonoBehaviour, IValidHit
{
    [SerializeField]
    private int _maxValue = 10;

    public void TriggerHitEffect(HitInfo info)
    {
        var magnitude = info.CollisionData.rigidbody.velocity.magnitude;

        var impactValue = Mathf.Clamp(info.ImpactDotProduct, 0, 1);
        var directionValue = Mathf.Clamp(info.DirectionDotProduct, 0, 1);
        var magnitudeBonusValue = magnitude * .25f * _maxValue;

        var hitValue = Mathf.RoundToInt(impactValue * directionValue * _maxValue + magnitudeBonusValue);

        if (ScoringManager.Instance != null)
        {
            ScoringManager.Instance.AddToScore(hitValue);
        }
    }
}