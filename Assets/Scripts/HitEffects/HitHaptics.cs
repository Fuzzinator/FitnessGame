using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitHaptics : MonoBehaviour, IValidHit
{
    //[SerializeField]
    private float _amplitude = .4f;
    [SerializeField]
    private float _effectLength = .15f;

    public void TriggerHitEffect(HitInfo info)
    {
        var baseline = info.MagnitudeBonus * info.HitQuality;

        var amplitude = Mathf.Clamp(baseline, _amplitude, 1f);
        var length = Mathf.Clamp(.25f * baseline, _effectLength, .25f);
        if (info.RightHand != null)
        {
            info.RightHand.SendHapticPulse(amplitude, length);
        }

        if (info.LeftHand != null)
        {
            info.LeftHand.SendHapticPulse(amplitude, length);
        }
    }
    
}
