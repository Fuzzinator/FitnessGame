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

    [SerializeField]
    protected float _minHitSpeed = 1f;

    protected bool _wasHit = false;
    public bool WasHit => _wasHit;
    public PoolManager MyPoolManager { get; set; }
    public Vector3 OptimalHitPoint { get; private set; }

    public FormationHolder parentFormation { get; private set; }

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
            var currentDistance = Vector3.Distance(transform.position, OptimalHitPoint);
            var hitInfo = new HitInfo(impactDotProduct, dirDotProduct, hand, other, currentDistance,
                hand.MovementSpeed);
            _successfulHitEvent?.Invoke(hitInfo);
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
        parentFormation.Remove(this);
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
        
#if UNITY_EDITOR
        return true;
#else
        return impactDotProd > _minMaxAllowance.x && hand.MovementSpeed>_minHitSpeed; // && dirDotProd > _minMaxAllowance.x;
#endif
    }

    public virtual void SetUpTarget(HitSideType hitSideType, Vector3 hitPoint, FormationHolder holder)
    {
        _noteType = hitSideType;
        _wasHit = false;
        _targetCreated?.Invoke(_noteType);
        parentFormation = holder;
        OptimalHitPoint = hitPoint;
    }
}