using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TargetSFX : MonoBehaviour, IValidHit, IMissedHit
{

    [SerializeField]
    private string _hitSound;

    [SerializeField]
    private string _missSound;

    public void TriggerHitEffect(HitInfo info)
    {
        if (string.IsNullOrWhiteSpace(_hitSound))
        {
            return;
        }
        SoundManager.PlaySound(_hitSound);
    }
    
    public void TriggerMissEffect()
    {
        if (string.IsNullOrWhiteSpace(_missSound))
        {
            return;
        }
        SoundManager.PlaySound(_missSound);
    }
}
