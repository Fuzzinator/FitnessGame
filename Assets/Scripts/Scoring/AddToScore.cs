using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddToScore : MonoBehaviour, IValidHit
{
    [SerializeField]
    private int _minValue = 1;

    [SerializeField]
    private int _maxValue = 25;

    [SerializeField]
    private float _hitRange = 2f;

    private const int OPTIMALHITMOD = 2;
    private const int MAXPUNCHSPEED = 30;

    private const float EASYMODIFIER = .5f;
    private const float NORMALMODIFIER = 1f;
    private const float HARDMODIFIER = 1.5f;
    private const float EXPERTMODIFER = 2f;


    public void TriggerHitEffect(HitInfo info)
    {
        var impactValue = Mathf.Clamp(info.ImpactDotProduct, 0, 1);
        var directionValue = Mathf.Clamp(info.DirectionDotProduct, 0, 1);
        var magnitudeBonusValue = Mathf.Clamp(info.HitSpeed, 0,MAXPUNCHSPEED) * .1f;
        var valueAsFloat = (impactValue + directionValue) * _minValue + magnitudeBonusValue;

        var hitValue = Mathf.Clamp(valueAsFloat * GetOptimalHitModifier(info.DistanceFromOptimalHit), _minValue, _maxValue);

        if (ScoringManager.Instance != null)
        {
            hitValue *= GetDifficultyModifier();
            ScoringManager.Instance.AddToScore(hitValue);
        }
    }

    private float GetOptimalHitModifier(float distance)
    {
        distance = Mathf.Clamp(distance, 0, _hitRange);
        distance /= _hitRange;
        return OPTIMALHITMOD - distance;
    }

    private static float GetDifficultyModifier()
    {
        var difficulty = SongInfoReader.Instance.Difficulty;
        return true switch
        {
            true when difficulty <= DifficultyInfo.EASY => EASYMODIFIER,
            true when difficulty <= DifficultyInfo.NORMAL => NORMALMODIFIER,
            true when difficulty <= DifficultyInfo.HARD => HARDMODIFIER,
            true when difficulty <= DifficultyInfo.EXPERT => EXPERTMODIFER,
            _ => NORMALMODIFIER
        };
    }
}