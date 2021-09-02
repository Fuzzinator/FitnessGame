using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HitInfo
{
    public float ImpactDotProduct { get; private set; }
    public float DirectionDotProduct { get; private set; }
    
    public Hand HitHand { get; private set; }
    public Collision CollisionData { get; private set; }

    public HitInfo(float impact, float direction, Hand hand,Collision collision)
    {
        ImpactDotProduct = impact;
        DirectionDotProduct = direction;
        HitHand = hand;
        CollisionData = collision;
    }
}
