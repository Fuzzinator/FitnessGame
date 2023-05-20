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
    protected Vector3 _optimalHitDirection;

    [SerializeField]
    protected Vector2 _minMaxAllowance;

    //[SerializeField]
    protected float _minHitSpeed = 1f;

    private float _overrideHitSpeed = -1f;

    protected bool _wasHit = false;

    protected List<ITargetInitializer> _targetInitializers = new List<ITargetInitializer>();
    protected List<IValidHit> _validHitEffects = new List<IValidHit>();
    protected List<IMissedHit> _missedHitEffects = new List<IMissedHit>();
    protected List<IBadHit> _badHitEffects = new List<IBadHit>();

    private const string PrecisionMode = "PrecisionMode";

    public HitSideType HitSideType => _noteType;
    public ChoreographyNote.CutDirection CutDirection => _cutDirection;
    public bool WasHit => _wasHit;
    public virtual bool IsSuperNote =>
            SettingsManager.GetCachedBool("AllowSuperStrikeTargets", true) && _overrideHitSpeed > 0f;

    public PoolManager MyPoolManager { get; set; }
    public Vector3 OptimalHitPoint { get; protected set; }

    public FormationHolder parentFormation { get; protected set; }

    public SetRendererMaterial RendererSetter => _setMaterial;

    private int _nameLayer;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var color = Gizmos.color;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.TransformPoint(_optimalHitDirection));
        Gizmos.color = color;
    }
#endif

    public void Initialize()
    {
        GetComponents(_targetInitializers);
        GetComponents(_validHitEffects);
        GetComponents(_missedHitEffects);
        GetComponents(_badHitEffects);
        _nameLayer = LayerMask.NameToLayer("Hand");
        //_optimalHitIndicator.Initialize();
    }

    public bool IsPooled { get; set; }

    protected virtual void OnCollisionEnter(Collision other)
    {
        if (!IsHit(other.collider, out var hand))
        {
            _wasHit = false;
            return;
        }

        var currentDistance = Vector3.Distance(transform.position, OptimalHitPoint);
        if (IsValidDirection(other, hand, out var impactDotProduct, out var dirDotProduct, out var handDotProd, out ValidHit validHit))
        {
            _wasHit = true;
            var hitInfo = new HitInfo(impactDotProduct, dirDotProduct, handDotProd, hand, currentDistance,
                hand.MovementSpeed);

            foreach (var hitEffect in _validHitEffects)
            {
                hitEffect.TriggerHitEffect(hitInfo);
            }
        }
        else
        {
            //var missInfo = new MissInfo();
            var hitInfo = new HitInfo(impactDotProduct, dirDotProduct, handDotProd, hand, currentDistance,
                hand.MovementSpeed);
            foreach (var hitEffect in _badHitEffects)
            {
                hitEffect.TriggerBadHitEffect(hitInfo, validHit);
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
        if (parentFormation != null)
        {
            parentFormation.Remove(this);
        }

        gameObject.SetActive(false);
        if (MyPoolManager.poolParent.gameObject.activeSelf)
        {
            transform.SetParent(MyPoolManager.poolParent);
        }

        ActiveTargetManager.Instance.RemoveActiveTarget(this);
        MyPoolManager.ReturnToPool(this);
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

    protected bool IsValidDirection(Collision other, Hand hand, out float impactDotProd, out float dirDotProd,
        out float handDotProd, out ValidHit validHit)
    {
        var precisionMode = SettingsManager.GetCachedBool(PrecisionMode, false);
        var handDirection = transform.InverseTransformDirection(hand.MovementDirection);
        var isSwinging = !precisionMode || hand.IsSwinging();
        impactDotProd = Vector3.Dot(-handDirection, _optimalHitDirection);
        var collisionDirection = Vector3.Normalize(other.GetContact(0).point - transform.position);
        dirDotProd = Vector3.Dot(collisionDirection, _optimalHitDirection);
        handDotProd = Vector3.Dot(transform.TransformDirection(_optimalHitDirection), hand.ForwardDirection);
        var requiredSpeed = IsSuperNote ? _overrideHitSpeed : _minHitSpeed;
        validHit = new ValidHit(isSwinging, impactDotProd > _minMaxAllowance.x, hand.MovementSpeed > requiredSpeed, !precisionMode || handDotProd < -.5f);
#if UNITY_EDITOR
        return true;
#else
        return validHit.IsValidHit;
#endif
    }

    public virtual void SetUpTarget(HitSideType hitSideType, Vector3 hitPoint, FormationHolder holder, float overrideHitSpeed = -1f)
    {
        _noteType = hitSideType;
        _wasHit = false;
        parentFormation = holder;
        OptimalHitPoint = hitPoint;
        _minHitSpeed = SettingsManager.GetMinHitSpeed(hitSideType);
        _overrideHitSpeed = overrideHitSpeed;
        foreach (var initializer in _targetInitializers)
        {
            initializer.Initialize(this);
        }
    }
}