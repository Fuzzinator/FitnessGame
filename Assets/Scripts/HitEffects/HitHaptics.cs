using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitHaptics : MonoBehaviour, IValidHit
{
    [SerializeField]
    private float _effectLength = .5f;

    public void TriggerHitEffect(HitInfo info)
    {
        var amplitude = Mathf.Clamp(info.DirectionDotProduct * info.ImpactDotProduct, .5f, 1f);
        if (info.Hands == null)
        {
            return;
        }
        for (int i = 0; i < info.Hands.Length; i++)
        {
            info.Hands[i].SendHapticPulse(amplitude, _effectLength);
        }
    }
}
