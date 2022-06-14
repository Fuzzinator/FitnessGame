using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseTarget : MonoBehaviour, IPoolable
{
    [SerializeField]
    public ChoreographyNote.LineLayerType layer;

    [SerializeField]
    protected HitSideType _noteType;

    [SerializeField]
    protected ChoreographyNote.CutDirection _cutDirection;

    [SerializeField]
    private SetRendererMaterial _setMaterial;
    
    [SerializeField]
    private BaseOptimalHitIndicator _optimalHitIndicator;
    
    [SerializeField]
    protected Vector3 _optimalHitDirection;

    [SerializeField]
    protected Vector2 _minMaxAllowance;

    [SerializeField]
    protected float _minHitSpeed = 1f;

    protected bool _wasHit = false;

    protected IValidHit[] _validHitEffects;
    protected IMissedHit[] _missedHitEffects;
    public bool WasHit => _wasHit;
    public PoolManager MyPoolManager { get; set; }
    public Vector3 OptimalHitPoint { get; protected set; }

    public FormationHolder parentFormation { get; protected set; }

    private int _nameLayer;

    public void Initialize()
    {
        _validHitEffects = GetComponents<IValidHit>();
        _missedHitEffects = GetComponents<IMissedHit>();
        _nameLayer = LayerMask.NameToLayer("Hand");
        _optimalHitIndicator.Initialize();
    }

    public bool IsPooled { get; set; }

    protected virtual void OnCollisionEnter(Collision other)
    { 
        if (!IsHit(other.collider, out var hand))
        {
            _wasHit = false;
            return;
        }

        if (IsValidDirection(other, hand, out var impactDotProduct, out var dirDotProduct))
        {
            _wasHit = true;
            var currentDistance = Vector3.Distance(transform.position, OptimalHitPoint);
            var hitInfo = new HitInfo(impactDotProduct, dirDotProduct, hand, currentDistance,
                hand.MovementSpeed);

            foreach (var hitEffect in _validHitEffects)
            {
                hitEffect.TriggerHitEffect(hitInfo);
            }
        }
    }

    public void Complete()
    {
        if (!_wasHit)
        {
            if (_missedHitEffects != null)
            {
                foreach (var missEffect in _missedHitEffects)
                {
                    missEffect.TriggerMissEffect();
                }
            }
        }
        ReturnToPool();
    }

    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        if (MyPoolManager.poolParent.gameObject.activeSelf)
        {
            transform.SetParent(MyPoolManager.poolParent);
        }

        ActiveTargetManager.Instance.RemoveActiveTarget(this);
        MyPoolManager.ReturnToPool(this);
        if (parentFormation != null)
        {
            parentFormation.Remove(this);
        }
    }

    protected bool IsHit(Collider col, out Hand hand)
    {
        if (col.gameObject.layer != _nameLayer)
        {
            hand = null;
            return false;
        }

        var hasHand = HandTracker.TryGetHand(col, out hand);
        if (!hasHand)
        {
            return false;
        }

        if (_noteType == HitSideType.Block)
        {
            return true;
        }

        return hand.AssignedHand == _noteType;
    }

    protected bool IsValidDirection(Collision other, Hand hand, out float impactDotProd, out float dirDotProd)
    {
        var handDirection = Vector3.Normalize(transform.InverseTransformDirection(hand.MovementDirection));
        impactDotProd = Vector3.Dot(-handDirection, _optimalHitDirection);
        var collisionDirection = Vector3.Normalize(other.GetContact(0).point - transform.position);
        dirDotProd = Vector3.Dot(collisionDirection, _optimalHitDirection);
        return impactDotProd > _minMaxAllowance.x && hand.MovementSpeed>_minHitSpeed;
    }

    public virtual void SetUpTarget(HitSideType hitSideType, Vector3 hitPoint, FormationHolder holder)
    {
        _noteType = hitSideType;
        _wasHit = false;
        parentFormation = holder;
        OptimalHitPoint = hitPoint;
        _setMaterial.Initialize(hitSideType);
    }
}