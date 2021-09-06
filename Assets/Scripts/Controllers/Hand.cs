using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Hand : MonoBehaviour
{
    [SerializeField]
    private HitSideType _assignedHand;

    public HitSideType AssignedHand => _assignedHand;

    public Vector3 MovementDirection => Vector3.Normalize(transform.position - _previousPosition);
    private Vector3 _previousPosition;

    private List<InputDevice> _devices = new List<InputDevice>();

    private void Start()
    {
        UpdateDevices();
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
            UnityEngine.XR.HapticCapabilities capabilities;
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

    private void LateUpdate()
    {
        _previousPosition = transform.position;
    }
}