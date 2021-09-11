using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockTarget : BaseTarget
{
    private bool _hitLeft = false;
    private bool _hitRight = false;

    protected override void OnCollisionEnter(Collision other)
    {
        if (!IsHit(other.collider, out var hand))
        {
            _wasHit = false;
            return;
        }
        
        _collidedEvent?.Invoke(other);

        switch (hand.AssignedHand)
        {
            case HitSideType.Left:
                _hitLeft = true;
                break;
            case HitSideType.Right:
                _hitRight = true;
                break;
            default:
                Debug.LogWarning("Well that's weird. What hit?");
                break;
        }

        if (_hitLeft && _hitRight)
        {
            _wasHit = true;
            _successfulHitEvent?.Invoke(new HitInfo(1, 1, hand, other));
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
                break;
            case HitSideType.Right:
                _hitRight = false;
                break;
            default:
                Debug.LogWarning("Well that's weird. What hit?");
                break;
        }
    }

    public override void SetUpTarget(HitSideType hitSideType)
    {
        _hitLeft = false;
        _hitRight = false;
        base.SetUpTarget(hitSideType);
    }
}
