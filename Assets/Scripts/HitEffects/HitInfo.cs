using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HitInfo
{
    public float ImpactDotProduct { get; private set; }
    public float DirectionDotProduct { get; private set; }

    public Hand HitHand => RightHand != null ? RightHand : LeftHand;

    private Hand _leftHand;
    public Hand LeftHand => _leftHand;

    private Hand _rightHand;
    public Hand RightHand => _rightHand;
    // Collision CollisionData { get; private set; }
    public float DistanceFromOptimalHit { get; private set; }

    public float HitSpeed { get; private set; }

    public HitInfo(float impact, float direction, Hand hand, float distance, float speed)
    {
        ImpactDotProduct = impact;
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
    }

    public HitInfo(float impact, float direction, IReadOnlyList<Hand> hands, float distance,
        float speed)
    {
        ImpactDotProduct = impact;
        DirectionDotProduct = direction;
        
        _rightHand = null;
        _leftHand = null;
        
        foreach (var hand in hands)
        {
            switch (hand.AssignedHand)
            {
                case HitSideType.Right:
                    _rightHand = hand;
                    break;
                case HitSideType.Left:
                    _leftHand = hand;
                    break;
            }
        }

        DistanceFromOptimalHit = distance;
        HitSpeed = speed;
    }
}