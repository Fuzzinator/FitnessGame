using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

public class Hand : BaseGameStateListener
{
    [SerializeField]
    private HitSideType _assignedHand;

    [SerializeField]
    private Transform _glove;

    [SerializeField]
    private Transform _uiRaycaster;

    public Collider MyCollider => _gloveController?.GloveCollider;

    public HitSideType AssignedHand => _assignedHand;

    public Vector3 GloveOffset
    {
        get => _glove.localPosition;
        set
        {
            _glove.localPosition = value;
            //#if !UNITY_ANDROID
            _uiRaycaster.localPosition = value;
            //#endif
        }
    }

    public Quaternion GloveRotationOffset
    {
        get => _glove.localRotation;
        set
        {
            _glove.localRotation = value;
            //#if !UNITY_ANDROID
            _uiRaycaster.localRotation = value;
            //#endif
        }
    }

    public Vector3 ForwardDirection => _glove.forward;

    public Vector3 MovementDirection
    {
        get
        {
            var direction = Vector3.zero;
            foreach (var dir in _previousDirections)
            {
                direction += dir;
            }

            return direction / _previousDirections.Length;
        }
    }

    public float MovementSpeed
    {
        get
        {
            //if (_devices == null || _devices.Count == 0)
            //{
            var speed = 0f;
            foreach (var previousSpeed in _previousSpeeds)
            {
                speed += previousSpeed;
            }

            return speed / _previousSpeeds.Length;
            /*}
            else
            {
                _devices[0].TryGetFeatureValue(CommonUsages.deviceAcceleration, out var acceleration)
                return acceleration.magnitude;
            }*/
        }
    }

    public Quaternion DefaultRotation
    {
        get
        {
            if (_defaultRotation == Quaternion.identity)
            {
                var controllerName = (_devices.Count > 0 ? _devices[0].name : null);
                _defaultRotation = SettingsManager.GetDefaultControllerRotation(controllerName);
            }

            return _defaultRotation;
        }
    }

    private Vector3 _previousPosition;

    private Vector3[] _previousDirections = new Vector3[3];
    private float[] _previousSpeeds = new float[3];

    private int _index = 0;

    private Quaternion _defaultRotation = Quaternion.identity;
    private GloveController _gloveController;
    private List<InputDevice> _devices = new List<InputDevice>();
    private bool _trackingPaused = false;
    private CancellationToken _cancellationToken;


    private void Awake()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateDevices();
        ProfileManager.Instance.activeProfileUpdated.AddListener(SetOffset);
        SetOffset();
        TrackDirAndSpeed(_cancellationToken).Forget();
        if (_devices.Count == 0)
        {
            InputDevices.deviceConnected += DeviceConnected;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        InputDevices.deviceDisconnected -= DeviceConnected;
        ProfileManager.Instance.activeProfileUpdated.RemoveListener(SetOffset);
    }


    protected override void GameStateListener(GameState oldState, GameState newState)
    {
        if (newState != oldState && oldState == GameState.Unfocused)
        {
            SetGlovesVisible(true);
        }

        switch (newState)
        {
            case GameState.Paused:
                _trackingPaused = true;
                break;
            case GameState.Unfocused:
                SetGlovesVisible(false);
                _trackingPaused = true;
                break;
            case GameState.Playing:
            case GameState.InMainMenu:
            case GameState.PreparingToPlay:
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

    public void SendMissedHaptics()
    {
        foreach (var device in _devices)
        {
            HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    uint channel = 0;
                    device.SendHapticBuffer(channel, HapticsController.Instance.MissHaptics.Samples);
                }
            }
        }
    }

    private async UniTask TrackDirAndSpeed(CancellationToken token)
    {
        while (enabled)
        {
            _previousPosition = transform.position;
            await UniTask.DelayFrame(1, cancellationToken: token);
            if (_cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (_trackingPaused)
            {
                continue;
            }

            var position = transform.position;

            _previousDirections[_index] = Vector3.Normalize(position - _previousPosition);
            _previousSpeeds[_index] = Vector3.Distance(position, _previousPosition) / Time.unscaledDeltaTime;

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

    private void SetOffset()
    {
        if (_glove == null)
        {
            return;
        }

        GloveOffset = _assignedHand switch
        {
            HitSideType.Left => SettingsManager.GetCachedVector3(SettingsManager.LEFTGLOVEOFFSET, Vector3.zero),
            HitSideType.Right => SettingsManager.GetCachedVector3(SettingsManager.RIGHTGLOVEOFFSET, Vector3.zero),
            _ => GloveOffset
        };

        GloveRotationOffset = _assignedHand switch
        {
            HitSideType.Left => SettingsManager.GetCachedQuaternion(SettingsManager.LEFTGLOVEROTOFFSET, DefaultRotation),
            HitSideType.Right => SettingsManager.GetCachedQuaternion(SettingsManager.RIGHTGLOVEROTOFFSET, DefaultRotation),
            _ => GloveRotationOffset
        };
    }

    public bool IsSwinging()
    {
        var dot = Vector3.Dot(_glove.forward, MovementDirection);
        return dot > .65f;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var color = Gizmos.color;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (_glove ? _glove.forward : transform.forward));


        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + MovementDirection);
        Gizmos.color = color;
    }
#endif


    public void SetAndSpawnGlove(GloveController newGlove)
    {
        _gloveController = Instantiate(newGlove, transform);

        _glove = _gloveController.transform;
        SetGloveColor();
        SetOffset();
        _glove?.gameObject.SetActive(true);
    }

    public void HideGlove()
    {
        _glove?.gameObject.SetActive(false);
    }

    private void SetGlovesVisible(bool visible)
    {
        _glove?.gameObject.SetActive(visible);
        _uiRaycaster.gameObject.SetActive(visible);
    }

    private void SetGloveColor()
    {
        var color = ColorsManager.Instance.GetAppropriateColor(_assignedHand, true);
        _gloveController.SetGloveColor(color);
    }

    public void UnparentGlove()
    {
        _glove?.SetParent(null);
        _uiRaycaster.SetParent(null);
    }

    public void ParentGlove()
    {
        _glove?.SetParent(transform);
        _uiRaycaster.SetParent(transform);
    }

    private void DeviceConnected(InputDevice device)
    {
        UpdateDevices();
        SetOffset();
    }
}