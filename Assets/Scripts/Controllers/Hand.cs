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

    [SerializeField]
    private Collider _collider;

    [SerializeField]
    private Transform _glove;

    private Renderer _gloveRenderer;
    
    public Collider MyCollider => _collider;

    public HitSideType AssignedHand => _assignedHand;

    public Vector3 GloveOffset
    {
        get => _glove.localPosition;
        set => _glove.localPosition = value;
    }

    public Quaternion GloveRotationOffset
    {
        get => _glove.localRotation;
        set => _glove.localRotation = value;
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
            return direction/_previousDirections.Length;
        }
    }

    public float MovementSpeed
    {
        get
        {
            var speed = 0f;
            foreach (var previousSpeed in _previousSpeeds)
            {
                speed += previousSpeed;
            }

            return speed / _previousSpeeds.Length;
        }
    }

    private Vector3 _previousPosition;
    
    private Vector3[] _previousDirections = new Vector3[3];
    private float[] _previousSpeeds = new float[3];
    
    private int _index = 0;

    private List<InputDevice> _devices = new List<InputDevice>();
    private bool _trackingPaused = false;
    private CancellationToken _cancellationToken;
    

    private void Awake()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    private void OnEnable()
    {
        //enabled = true;
        UpdateDevices();
        
        SetOffset();
        TrackDirAndSpeed(_cancellationToken).Forget();
    }

    /*protected override  void OnDisable()
    {
        base.OnDisable();
        enabled = false;
    }*/

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

    private async UniTask TrackDirAndSpeed(CancellationToken token)
    {
        while (enabled)
        {
            _previousPosition = transform.position;
            await UniTask.DelayFrame(1, cancellationToken:token);
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
            _previousSpeeds[_index] = Vector3.Distance(position, _previousPosition)/Time.unscaledDeltaTime;
            
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
            HitSideType.Left => SettingsManager.GetSetting(SettingsManager.LEFTGLOVEOFFSET, Vector3.zero),
            HitSideType.Right => SettingsManager.GetSetting(SettingsManager.RIGHTGLOVEOFFSET, Vector3.zero),
            _ => GloveOffset
        };
        GloveRotationOffset = _assignedHand switch
        {
            HitSideType.Left => SettingsManager.GetSetting(SettingsManager.LEFTGLOVEROTOFFSET, SettingsManager.DEFAULTGLOVEROTATION),
            HitSideType.Right => SettingsManager.GetSetting(SettingsManager.RIGHTGLOVEROTOFFSET, SettingsManager.DEFAULTGLOVEROTATION),
            _ => GloveRotationOffset
        };
    }

    public bool IsSwinging()
    {
        //return true;
        //TODO Come back to this someday and figure it out
        var dot = Vector3.Dot(_glove.forward, MovementDirection);
        return dot > .65f;
    }
    
    public void SetAndSpawnGlove(Collider newGlove)
    {
        _collider = Instantiate(newGlove, transform);

        _collider.TryGetComponent(out _gloveRenderer);
        _glove = _collider.transform;
        SetGloveColor();
        SetOffset();
    }

    private void SetGloveColor()
    {
        _gloveRenderer.sharedMaterial.color = ColorsManager.Instance.GetAppropriateColor(_assignedHand, true);
    }
    
    public void UnparentGlove()
    {
        _glove.SetParent(null);
    }

    public void ParentGlove()
    {
        _glove.SetParent(transform);
    }
}