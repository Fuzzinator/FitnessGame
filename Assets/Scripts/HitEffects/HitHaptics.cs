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

        if (info.RightHand != null)
        {
            info.RightHand.SendHapticPulse(amplitude, _effectLength);
        }

        if (info.LeftHand != null)
        {
            info.LeftHand.SendHapticPulse(amplitude, _effectLength);
        }
    }
}
