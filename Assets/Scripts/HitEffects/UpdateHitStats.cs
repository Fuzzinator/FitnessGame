using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateHitStats : MonoBehaviour, IValidHit, IMissedHit, IBadHit
{    
    public void TriggerHitEffect(HitInfo info)
    {
        ScoringAndHitStatsManager.Instance.RegisterHitTarget(info);
        ScoringAndHitStatsManager.Instance.RecordHitSpeed(info, true);
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
        ScoringAndHitStatsManager.Instance.RecordHitSpeed(info, false);
    }
}
