using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockTarget : BaseTarget
{
    private bool _hitLeft = false;
    private bool _hitRight = false;

    private Hand _leftHand;
    private Hand _rightHand;

    protected override void OnCollisionEnter(Collision other)
    {
        if (!IsHit(other.collider, out var hand))
        {
            _wasHit = false;
            return;
        }

        switch (hand.AssignedHand)
        {
            case HitSideType.Left:
                _hitLeft = true;
                _leftHand = hand;
                break;
            case HitSideType.Right:
                _hitRight = true;
                _rightHand = hand;
                break;
            default:
                Debug.LogWarning("Well that's weird. What hit?");
                break;
        }

        if (_hitLeft && _hitRight)
        {
            _wasHit = true;

            var currentDistance = Vector3.Distance(transform.position, OptimalHitPoint);
            var hitInfo = new HitInfo(1, 1, _leftHand, _rightHand, currentDistance,
                hand.MovementSpeed);

            foreach (var hitEffect in _validHitEffects)
            {
                hitEffect.TriggerHitEffect(hitInfo);
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (!IsHit(other.collider, out var hand))
        {
            return;
        }

        switch (hand.AssignedHand)
        {
            case HitSideType.Left:
                _hitLeft = false;
                _leftHand = null;
                break;
            case HitSideType.Right:
                _hitRight = false;
                _rightHand = null;
                break;
            default:
                Debug.LogWarning("Well that's weird. What hit?");
                break;
        }
    }

    public override void SetUpTarget(HitSideType hitSideType, Vector3 hitPoint, FormationHolder holder)
    {
        _hitLeft = false;
        _hitRight = false;
        _rightHand = null;
        _leftHand = null;
        OptimalHitPoint = hitPoint;
        base.SetUpTarget(hitSideType, hitPoint, holder);
    }
}