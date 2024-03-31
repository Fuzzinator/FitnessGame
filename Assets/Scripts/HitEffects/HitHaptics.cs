using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HitHaptics : MonoBehaviour, IValidHit//, IBadHit
{
    //[SerializeField]
#if UNITY_ANDROID
    private float _amplitude = .4f;
#else
    private float _amplitude = 1f;
#endif
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

    public void TriggerBadHitEffect(HitInfo info)
    {
        if (info.RightHand != null)
        {
            info.RightHand.SendMissedHaptics();
        }

        if (info.LeftHand != null)
        {
            info.LeftHand.SendMissedHaptics();
        }
    }
    
}
