using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class UIAudioController : MonoBehaviour
{
    public static UIAudioController Instance { get; private set; }

    [SerializeField]
    private string _clickedSound;
    [SerializeField]
    private string _pressedSound;
    [SerializeField]
    private string _releasedSound;
    [SerializeField]
    private string _hoveredSound;
    [SerializeField]
    private string _unhoveredSound;
    [SerializeField]
    private AudioMixerGroup _audioMixer;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void Clicked()
    {
        PlaySound(_clickedSound);
    }

    public void PointerPressed()
    {
        PlaySound(_pressedSound);
    }

    public void PointerReleased()
    {
        PlaySound(_releasedSound);
    }

    public void Hovered()
    {
        PlaySound(_hoveredSound);
    }

    public void Unhovered()
    {
        PlaySound(_unhoveredSound);
    }
    
    private void PlaySound(string sound)
    {
        var soundSettings = new SoundManager.AudioSourceSettings(false, _audioMixer);
        SoundManager.PlaySound(sound, soundSettings);
    }
}
