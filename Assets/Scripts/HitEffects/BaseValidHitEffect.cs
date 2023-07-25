using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(BaseTarget))]
public class BaseValidHitEffect : MonoBehaviour, IValidHit
{
   [SerializeField]
   private BaseTarget _target;

   private void OnValidate()
   {
      _target = GetComponent<BaseTarget>();
   }

   public virtual void TriggerHitEffect(HitInfo info)
   {
      PoolTarget();
   }

   public virtual void PoolTarget()
   {
      _target.ReturnToPool();
   }
}
