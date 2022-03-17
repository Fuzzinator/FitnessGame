using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlaySounds : MonoBehaviour
{
    [SerializeField]
    private string[] _sounds;

    [SerializeField]
    private AudioMixerGroup _mixerGroup;

    public void PlaySound(int index)
    {
        var settings = new SoundManager.AudioSourceSettings(false, _mixerGroup);
        SoundManager.PlaySound(_sounds[index], settings);
    }
}
