using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

public class TargetSFX : MonoBehaviour, IValidHit, IMissedHit
{

    [SerializeField]
    private string _hitSound;

    [SerializeField]
    private string _missSound;

    [SerializeField]
    private AudioMixerGroup _audioMixer;

    public void TriggerHitEffect(HitInfo info)
    {
        if (string.IsNullOrWhiteSpace(_hitSound))
        {
            return;
        }
        PlaySound(_hitSound);
    }
    
    public void TriggerMissEffect()
    {
        if (string.IsNullOrWhiteSpace(_missSound))
        {
            return;
        }
        PlaySound(_missSound);
    }

    private void PlaySound(string sound)
    {
        var soundSettings = new SoundManager.AudioSourceSettings(false, _audioMixer);
        SoundManager.PlaySound(sound, soundSettings);
    }
}
