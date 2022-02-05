using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateHitStats : MonoBehaviour, IValidHit, IMissedHit
{
    [SerializeField]
    private bool _isTarget;
    
    public void TriggerHitEffect(HitInfo info)
    {
        ScoringManager.Instance.RegisterHit();
    }

    public void TriggerMissEffect()
    {
        ScoringManager.Instance.RegisterMiss(_isTarget);
    }
}
