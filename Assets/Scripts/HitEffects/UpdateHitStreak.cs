using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateHitStreak : MonoBehaviour, IValidHit, IMissedHit
{
    public void TriggerHitEffect(HitInfo info)
    {
        if (StreakManager.Instance == null)
        {
            return;
        }
        StreakManager.Instance.IncreaseStreak();
    }

    public void TriggerMissEffect()
    {
        if (StreakManager.Instance == null)
        {
            return;
        }
        StreakManager.Instance.ResetStreak();
    }
}
