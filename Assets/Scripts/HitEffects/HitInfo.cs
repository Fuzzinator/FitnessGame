using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct HitInfo
{
    [field:SerializeField]
    public float ImpactDotProduct { get; private set; }

    [field: SerializeField]
    public float HandDirectionDotProd { get; private set; }
    [field: SerializeField]
    public float DirectionDotProduct { get; private set; }

    public Hand HitHand => RightHand != null ? RightHand : LeftHand;

    private Hand _leftHand;
    public Hand LeftHand => _leftHand;

    private Hand _rightHand;
    public Hand RightHand => _rightHand;
    // Collision CollisionData { get; private set; }
    [field: SerializeField]
    public float DistanceFromOptimalHit { get; private set; }

    [field: SerializeField]
    public float HitSpeed { get; private set; }

    [field: SerializeField]
    public float HitQuality { get; private set; }

    [field: SerializeField]
    public float MagnitudeBonus { get; private set; }

    [field: SerializeField]
    public HitQualityName QualityName { get; private set; }

    private const float FootInMeters = 0.3048f;

    public HitInfo(float impact, float direction, float handDir, Hand hand, float distance, float speed)
    {
        ImpactDotProduct = impact;
        HandDirectionDotProd = handDir;
        DirectionDotProduct = direction;
        _rightHand = null;
        _leftHand = null;

        switch (hand.AssignedHand)
        {
            case HitSideType.Right:
                _rightHand = hand;
                break;
            case HitSideType.Left:
                _leftHand = hand;
                break;
        }

        DistanceFromOptimalHit = distance;
        HitSpeed = speed;
        HitQuality = GetModifierRange(impact, direction, distance);
        MagnitudeBonus = GetMagnitudeBonus(speed);
        QualityName = GetHitQualityName(HitQuality);
    }

    public HitInfo(float impact, float direction, float handDir, Hand leftHand, Hand rightHand, float distance,
        float speed)
    {
        ImpactDotProduct = impact;
        HandDirectionDotProd = handDir;
        DirectionDotProduct = direction;

        _rightHand = rightHand;
        _leftHand = leftHand;

        DistanceFromOptimalHit = distance;
        HitSpeed = speed;
        HitQuality = GetModifierRange(impact, direction, distance);
        MagnitudeBonus = GetMagnitudeBonus(speed);
        QualityName = GetHitQualityName(HitQuality);

    }

    private static float GetModifierRange(float impact, float direction, float distance)
    {
        var distanceValue = 1f - Mathf.Clamp(distance, 0, FootInMeters) * 3.333f;
        var impactValue = Mathf.Clamp(impact, 0, 1);
        var directionValue = Mathf.Clamp(direction, 0, 1);
        var final = (distanceValue + impactValue + directionValue)* .333f;
        return final;
    }

    private static float GetMagnitudeBonus(float speed)
    {
        return Mathf.Clamp(speed, 0, 30) * .5f;
    }

    private static HitQualityName GetHitQualityName(float hitQuality)
    {
        hitQuality = 1 - hitQuality;
        return hitQuality switch
        {
            _ when hitQuality < .2f => HitQualityName.Perfect,
            _ when hitQuality < .4f => HitQualityName.Great,
            _ when hitQuality < .6f => HitQualityName.Good,
            _ when hitQuality < .8f => HitQualityName.Okay,
            _ when hitQuality >= .8f => HitQualityName.Bad,
            _ => HitQualityName.Bad
        };
    }

    public enum HitQualityName
    {
        Bad = 0,
        Okay = 1,
        Good = 2,
        Great = 3,
        Perfect = 4
    }
}