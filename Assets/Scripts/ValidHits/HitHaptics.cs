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
        var amplitude = Mathf.Clamp(info.DirectionDotProduct * info.ImpactDotProduct, .25f, 1f);
        
        info.HitHand.SendHapticPulse(amplitude, _effectLength);
    }
}
