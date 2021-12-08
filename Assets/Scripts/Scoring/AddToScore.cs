using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddToScore : MonoBehaviour, IValidHit
{
    [SerializeField]
    private int _minValue = 5;

    [SerializeField]
    private int _maxValue = 25;

    [SerializeField]
    private float _hitRange = 2f;

    private const int OPTIMALHITMOD = 2;
    private const int MAXPUNCHSPEED = 30;


    public void TriggerHitEffect(HitInfo info)
    {
        var impactValue = Mathf.Clamp(info.ImpactDotProduct, 0, 1);
        var directionValue = Mathf.Clamp(info.DirectionDotProduct, 0, 1);
        var magnitudeBonusValue = Mathf.Clamp(info.HitSpeed, 1,MAXPUNCHSPEED) * .25f;

        var valueAsFloat = (impactValue + directionValue) * _minValue + magnitudeBonusValue;

        var hitValue = Mathf.Clamp(valueAsFloat * GetOptimalHitModifier(info.DistanceFromOptimalHit), _minValue, _maxValue);

        if (ScoringManager.Instance != null)
        {
            ScoringManager.Instance.AddToScore(Mathf.RoundToInt(hitValue*StreakManager.GetStreakScoreMod()));
        }
    }

    private float GetOptimalHitModifier(float distance)
    {
        distance = Mathf.Clamp(distance, 0, _hitRange);
        distance /= _hitRange;
        return OPTIMALHITMOD - distance;
    }
}