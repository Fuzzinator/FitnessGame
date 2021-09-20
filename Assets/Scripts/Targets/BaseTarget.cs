using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseTarget : MonoBehaviour, IPoolable
{
    [SerializeField]
    protected HitSideType _noteType;

    [SerializeField]
    protected ChoreographyNote.CutDirection _cutDirection;

    [SerializeField]
    protected UnityEvent<Collision> _collidedEvent;
    
    [SerializeField]
    protected UnityEvent _missedHitEvent = new UnityEvent();
    
    [SerializeField]
    protected UnityEvent<HitInfo> _successfulHitEvent;
    
    [SerializeField]
    protected UnityEvent<HitSideType> _targetCreated = new UnityEvent<HitSideType>();

    [SerializeField]
    protected Vector3 _optimalHitDirection;

    [SerializeField]
    protected Vector2 _minMaxAllowance;

    protected bool _wasHit = false;

    public PoolManager MyPoolManager { get; set; }

    public bool IsPooled { get; set; }
    protected virtual void OnCollisionEnter(Collision other)
    {
        if (!IsHit(other.collider, out var hand))
        {
            _wasHit = false;
            return;
        }

        _collidedEvent?.Invoke(other);
        
        if (IsValidDirection(other, hand, out var impactDotProduct, out var dirDotProduct))
        {
            _wasHit = true;
            _successfulHitEvent?.Invoke(new HitInfo(impactDotProduct,  dirDotProduct, hand, other));
        }
    }


    public void ReturnToPool()
    {
        if (!_wasHit)
        {
            _missedHitEvent?.Invoke();
        }
        gameObject.SetActive(false);
        transform.SetParent(MyPoolManager.poolParent);
        ActiveTargetManager.Instance.RemoveActiveTarget(this);
        MyPoolManager.ReturnToPool(this);
    }

    protected bool IsHit(Collider col, out Hand hand)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Hand"))
        {
            //var hasHand = col.TryGetComponent(out hand);
            hand = col.GetComponentInParent<Hand>();
            var hasHand = hand != null;
            if (_noteType == HitSideType.Block)
            {
                return true;
            }

            return (hasHand && hand.AssignedHand == _noteType);
        }

        hand = null;
        return false;
    }

    protected bool IsValidDirection(Collision other, Hand hand, out float impactDotProd, out float dirDotProd)
    {
        var handDirection = Vector3.Normalize(transform.InverseTransformDirection(hand.MovementDirection));
        impactDotProd = Vector3.Dot(-handDirection, _optimalHitDirection);
        var collisionDirection = Vector3.Normalize(other.contacts[0].point - transform.position);
        dirDotProd = Vector3.Dot(collisionDirection, _optimalHitDirection);
        return impactDotProd>_minMaxAllowance.x;// && dirDotProd > _minMaxAllowance.x;
    }

    public virtual void SetUpTarget(HitSideType hitSideType)
    {
        _noteType = hitSideType;
        _wasHit = false;
        _targetCreated?.Invoke(_noteType);
    }
}