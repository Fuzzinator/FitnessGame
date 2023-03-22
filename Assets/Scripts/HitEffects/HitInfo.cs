using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HitInfo
{
    public float ImpactDotProduct { get; private set; }

    public float HandDirectionDotProd { get; private set; }
    public float DirectionDotProduct { get; private set; }

    public Hand HitHand => RightHand != null ? RightHand : LeftHand;

    private Hand _leftHand;
    public Hand LeftHand => _leftHand;

    private Hand _rightHand;
    public Hand RightHand => _rightHand;
    // Collision CollisionData { get; private set; }
    public float DistanceFromOptimalHit { get; private set; }

    public float HitSpeed { get; private set; }

    public float HitQuality { get; private set; }

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
    }

    private static float GetModifierRange(float impact, float direction, float distance)
    {
        var distanceValue = 1f - Mathf.Clamp(distance, 0, FootInMeters) * 3.333f;
        var impactValue = Mathf.Clamp(impact, 0, 1);
        var directionValue = Mathf.Clamp(direction, 0, 1);
        var final = (distance + impactValue + directionValue) * .333f;
        //var magnitudeBonusValue = 1- Mathf.Clamp(info.HitSpeed, 0, 30) * .1f;
        return final;
    }
}