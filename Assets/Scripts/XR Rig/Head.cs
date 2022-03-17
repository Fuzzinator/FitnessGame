using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class Head : MonoBehaviour
{
    public static Head Instance { get; private set; }
    public Camera HeadCamera => _camera;
    
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private LayerMask _layerMask;
    
    [SerializeField]
    private string _flyBySound;

    [SerializeField]
    private LayerMask _flyByLayerMask;
    
    
    [SerializeField]
    private AudioMixerGroup _audioMixer;
    
    [SerializeField]
    protected UnityEvent _hitHeadEvent = new UnityEvent();

    private void Awake()
    {
        Instance = this;
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (IsHit(other, _layerMask))
        {
            _hitHeadEvent?.Invoke();
        }
        else if (IsHit(other, _flyByLayerMask))
        {
            TriggerFlyByEffect();
        }
    }

    protected bool IsHit(Collider col, LayerMask layerMask)
    {
        return layerMask == (layerMask.value | (1 << col.gameObject.layer));
    }

    private void TriggerFlyByEffect()
    {
        if (string.IsNullOrWhiteSpace(_flyBySound))
        {
            return;
        }

        var soundSettings = new SoundManager.AudioSourceSettings(false, _audioMixer);
        SoundManager.PlaySound(_flyBySound, soundSettings);
    }
}
