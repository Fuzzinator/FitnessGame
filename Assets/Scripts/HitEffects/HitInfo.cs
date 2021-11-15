using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HitInfo
{
    public float ImpactDotProduct { get; private set; }
    public float DirectionDotProduct { get; private set; }

    public Hand HitHand => Hands?[0];
    public Hand[] Hands { get; private set; }
    public Collision CollisionData { get; private set; }
    public float DistanceFromOptimalHit { get; private set; }

    public float HitSpeed { get; private set; }

    public HitInfo(float impact, float direction, Hand hand, Collision collision, float distance, float speed)
    {
        ImpactDotProduct = impact;
        DirectionDotProduct = direction;
        Hands = new[] {hand};
        CollisionData = collision;
        DistanceFromOptimalHit = distance;
        HitSpeed = speed;
    }

    public HitInfo(float impact, float direction, IReadOnlyList<Hand> hands, Collision collision, float distance,
        float speed)
    {
        ImpactDotProduct = impact;
        DirectionDotProduct = direction;
        Hands = new Hand[hands.Count];
        for (int i = 0; i < hands.Count; i++)
        {
            Hands[i] = hands[i];
        }

        CollisionData = collision;
        DistanceFromOptimalHit = distance;
        HitSpeed = speed;
    }
}