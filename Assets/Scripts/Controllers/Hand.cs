using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

public class Hand : BaseGameStateListener 
{
    [SerializeField]
    private HitSideType _assignedHand;

    public HitSideType AssignedHand => _assignedHand;

    public Vector3 MovementDirection
    {
        get
        {
            var direction = Vector3.zero;
            foreach (var dir in _previousDirections)
            {
                direction += dir;
            }
            return direction/_previousDirections.Length;
        }
    }

    private Vector3 _previousPosition;
    private Vector3[] _previousDirections = new Vector3[3];
    private int _index = 0;

    private List<InputDevice> _devices = new List<InputDevice>();
    private bool _enabled;
    private bool _trackingPaused = false;
    private async void OnEnable()
    {
        _enabled = true;
        UpdateDevices();
        await TrackDirection(this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
    }

    private void OnDisable()
    {
        _enabled = false;
    }

    protected override void GameStateListener(GameState oldState, GameState newState)
    {
        switch (newState)
        {
            case GameState.Paused:
            case GameState.Unfocused:
                _trackingPaused = true;
                break;
            case GameState.Playing:
            case GameState.InMainMenu:
                _trackingPaused = false;
                break;
        }
    }

    private void UpdateDevices()
    {
        var filter = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.TrackedDevice |
                     (_assignedHand == HitSideType.Left
                          ? InputDeviceCharacteristics.Left
                          : InputDeviceCharacteristics.Right);
        InputDevices.GetDevicesWithCharacteristics(filter, _devices);
    }

    public void SendHapticPulse(float amplitude, float duration)
    {
        foreach (var device in _devices)
        {
            HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    uint channel = 0;
                    device.SendHapticImpulse(channel, amplitude, duration);
                }
            }
        }
    }

    private async UniTask TrackDirection(CancellationToken token)
    {
        while (_enabled)
        {
            _previousPosition = transform.position;
            await UniTask.DelayFrame(1, cancellationToken:token);
            
            if (_trackingPaused)
            {
                continue;
            }
            
            _previousDirections[_index] = transform.position - _previousPosition;
            
            if (_index + 1 < _previousDirections.Length)
            {
                _index++;
            }
            else
            {
                _index = 0;
            }
        }
    }
}