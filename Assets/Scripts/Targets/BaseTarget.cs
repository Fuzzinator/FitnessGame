using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseTarget : MonoBehaviour, IPoolable
{
    [SerializeField]
    private HitSideType _noteType;

    [SerializeField]
    private ChoreographyNote.CutDirection _cutDirection;

    [SerializeField]
    private UnityEvent<Collision> _collidedEvent;
    
    [SerializeField]
    private UnityEvent<HitInfo> _successfulHitEvent;
    
    [SerializeField]
    private UnityEvent<HitSideType> _targetCreated = new UnityEvent<HitSideType>();

    [SerializeField]
    private Vector3 _optimalHitDirection;

    [SerializeField]
    private Vector2 _minMaxAllowance;

    protected void OnCollisionEnter(Collision other)
    {
        if (!IsHit(other.collider, out var hand))
        {
            return;
        }

        _collidedEvent?.Invoke(other);
        
        if (IsValidDirection(other, hand, out var impactDotProduct, out var dirDotProduct))
        {
            _successfulHitEvent?.Invoke(new HitInfo(impactDotProduct,  dirDotProduct, hand, other));
        }
    }

    public PoolManager MyPoolManager { get; set; }

    public bool IsPooled { get; set; }

    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        transform.SetParent(MyPoolManager.poolParent);
        ActiveTargetManager.Instance.RemoveActiveTarget(this);
        MyPoolManager.ReturnToPool(this);
    }

    private bool IsHit(Collider col, out Hand hand)
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

    private bool IsValidDirection(Collision other, Hand hand, out float impactDotProd, out float dirDotProd)
    {
        var handDirection = Vector3.Normalize(hand.MovementDirection);
        impactDotProd = Vector3.Dot(-handDirection, _optimalHitDirection);
        var collisionDirection = Vector3.Normalize(other.contacts[0].point - transform.position);
        dirDotProd = Vector3.Dot(collisionDirection, _optimalHitDirection);
        return impactDotProd>_minMaxAllowance.x;// && dirDotProd > _minMaxAllowance.x;
    }

    public void SetUpTarget(HitSideType hitSideType)
    {
        _noteType = hitSideType;
        _targetCreated?.Invoke(_noteType);
    }

    private float GetHitSpeed(Hand hand)
    {
        
        return 0;
    }
}