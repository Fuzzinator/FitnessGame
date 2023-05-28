using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateHitStats : MonoBehaviour, IValidHit, IMissedHit, IBadHit
{
    [SerializeField]
    private bool _recordHitSpeed = true;
    public void TriggerHitEffect(HitInfo info)
    {
        ScoringAndHitStatsManager.Instance.RegisterHitTarget(info);
        if(!_recordHitSpeed)
        {
            return;
        }
        ScoringAndHitStatsManager.Instance.RecordHitSpeed(info);
    }

    public void TriggerMissEffect()
    {
        ScoringAndHitStatsManager.Instance.RegisterMissedTarget();
    }

    public void HitObstacle(Collider hitObstacle)
    {
        ScoringAndHitStatsManager.Instance.RegisterHitObstacle();        
    }

    public void TriggerBadHitEffect(HitInfo info, ValidHit validHit)
    {
        if (!_recordHitSpeed)
        {
            return;
        }
        ScoringAndHitStatsManager.Instance.RecordHitSpeed(info);
    }
}
