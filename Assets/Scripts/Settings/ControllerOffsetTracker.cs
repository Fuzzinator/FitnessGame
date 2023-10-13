using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerOffsetTracker : MonoBehaviour, ISaver
{
    private const string LEFTGRIPPRESSED = "LeftGripPressed";
    private const string RIGHTGRIPPRESSED = "RightGripPressed";
    private const string LEFTGRIPRELEASED = "LeftGripReleased";
    private const string RIGHTGRIPRELEASED = "RightGripReleased";

    private bool _leftGripPressed;
    private bool _rightGripPressed;

    private Vector3 _leftStartPos;
    private Quaternion _leftStartRot;
    private Vector3 _rightStartPos;
    private Quaternion _rightStartRot;

    private Vector3 _leftBeginPos;
    private Quaternion _leftBeginRot;
    private Vector3 _rightBeginPos;
    private Quaternion _rightBeginRot;

    private Vector3 _leftOffset;
    private Quaternion _leftRotationOffset;
    private Vector3 _rightOffset;
    private Quaternion _rightRotationOffset;

    private CancellationToken _cancellationToken;

    public bool IsLeftGripPressed => _leftGripPressed;
    public bool IsRightGripPressed => _rightGripPressed;

    public bool SaveRequested { get; set; }

    private void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    private void OnEnable()
    {
        InputManager.Instance.MainInput[LEFTGRIPPRESSED].performed += LeftGripPressed;
        InputManager.Instance.MainInput[RIGHTGRIPPRESSED].performed += RightGripPressed;
        InputManager.Instance.MainInput[LEFTGRIPRELEASED].performed += LeftGripReleased;
        InputManager.Instance.MainInput[RIGHTGRIPRELEASED].performed += RightGripReleased;

        SaveRequested = false;

#if UNITY_EDITOR && UNITY_ANDROID
        _leftStartPos = HandTracker.LeftEditorHand.GloveOffset;
        _rightStartPos = HandTracker.RightEditorHand.GloveOffset;
#else
        _leftStartPos = HandTracker.LeftHand.GloveOffset;
        _rightStartPos = HandTracker.RightHand.GloveOffset;
        
#endif
    }

    private void OnDisable()
    {
        InputManager.Instance.MainInput[LEFTGRIPPRESSED].performed -= LeftGripPressed;
        InputManager.Instance.MainInput[RIGHTGRIPPRESSED].performed -= RightGripPressed;
        InputManager.Instance.MainInput[LEFTGRIPRELEASED].performed -= LeftGripReleased;
        InputManager.Instance.MainInput[RIGHTGRIPRELEASED].performed -= RightGripReleased;

        if (!SaveRequested)
        {
            Revert();
        }
    }

    private void LeftGripPressed(InputAction.CallbackContext obj)
    {
        _leftGripPressed = true;
#if UNITY_EDITOR && UNITY_ANDROID

        TrackControllerPosition(HandTracker.LeftEditorHand);
#else
        TrackControllerPosition(HandTracker.LeftHand);
#endif
    }

    private void RightGripPressed(InputAction.CallbackContext obj)
    {
        _rightGripPressed = true;
#if UNITY_EDITOR && UNITY_ANDROID
        TrackControllerPosition(HandTracker.RightEditorHand);
#else
        TrackControllerPosition(HandTracker.RightHand);
#endif
    }

    private void LeftGripReleased(InputAction.CallbackContext obj)
    {
        _leftGripPressed = false;
        var leftHand = HandTracker.LeftHand;
#if UNITY_EDITOR && UNITY_ANDROID
        leftHand = HandTracker.LeftEditorHand;
#endif
        leftHand.ParentGlove();
        _leftOffset = leftHand.GloveOffset;
        _leftRotationOffset = leftHand.GloveRotationOffset;
    }

    private void RightGripReleased(InputAction.CallbackContext obj)
    {
        _rightGripPressed = false;
        var rightHand = HandTracker.RightHand;
#if UNITY_EDITOR && UNITY_ANDROID
        rightHand = HandTracker.RightEditorHand;
#endif
        rightHand.ParentGlove();
        _rightOffset = rightHand.GloveOffset;
        _rightRotationOffset = rightHand.GloveRotationOffset;
    }

    private void TrackControllerPosition(Hand hand)
    {
        var isLeftHand = hand.AssignedHand == HitSideType.Left;
        var handTransform = hand.transform;
        if (isLeftHand)
        {
            _leftBeginPos = handTransform.localPosition;
            _leftBeginRot = handTransform.localRotation;
        }
        else
        {
            _rightBeginPos = handTransform.localPosition;
            _rightBeginRot = handTransform.localRotation;
        }

        hand.UnparentGlove();
    }

    public void Finish()
    {
        SaveRequested = true;
        SettingsDisplay.Instance.ChangeWasMade(this);
    }

    public void CancelChanges()
    {
        SaveRequested = false;
        Revert();
    }

    public void Save(Profile overrideProfile = null)
    {
        SettingsManager.SetSetting(SettingsManager.LEFTGLOVEOFFSET, _leftOffset);
        SettingsManager.SetSetting(SettingsManager.RIGHTGLOVEOFFSET, _rightOffset);
        SettingsManager.SetSetting(SettingsManager.LEFTGLOVEROTOFFSET, _leftRotationOffset);
        SettingsManager.SetSetting(SettingsManager.RIGHTGLOVEROTOFFSET, _rightRotationOffset);
        SaveRequested = false;
    }

    public void ResetLeftController()
    {
#if UNITY_EDITOR && UNITY_ANDROID
        ResetController(HandTracker.LeftEditorHand);
#else
        ResetController(HandTracker.LeftHand);
#endif
    }

    public void ResetRightController()
    {
#if UNITY_EDITOR && UNITY_ANDROID
        ResetController(HandTracker.RightEditorHand);
#else
        ResetController(HandTracker.RightHand);
#endif
    }

    private void ResetController(Hand hand)
    {
        var isLeftHand = hand.AssignedHand == HitSideType.Left;

        hand.GloveOffset = Vector3.zero;
        hand.GloveRotationOffset = hand.DefaultRotation;

        if (isLeftHand)
        {
            _leftOffset = hand.GloveOffset;
            _leftRotationOffset = hand.GloveRotationOffset;
            SettingsManager.SetSetting(SettingsManager.LEFTGLOVEOFFSET, _leftOffset);
            SettingsManager.SetSetting(SettingsManager.LEFTGLOVEROTOFFSET, _leftRotationOffset);
        }
        else
        {
            _rightOffset = hand.GloveOffset;
            _rightRotationOffset = hand.GloveRotationOffset;
            SettingsManager.SetSetting(SettingsManager.RIGHTGLOVEOFFSET, _rightOffset);
            SettingsManager.SetSetting(SettingsManager.RIGHTGLOVEROTOFFSET, _rightRotationOffset);
        }
    }

    public void Revert()
    {
        _leftOffset = SettingsManager.GetSetting(SettingsManager.LEFTGLOVEOFFSET, Vector3.zero);
        _rightOffset = SettingsManager.GetSetting(SettingsManager.RIGHTGLOVEOFFSET, Vector3.zero);
#if UNITY_EDITOR && UNITY_ANDROID
        HandTracker.LeftEditorHand.GloveOffset = _leftOffset;
        HandTracker.RightEditorHand.GloveOffset = _rightOffset;
#else
        HandTracker.LeftHand.GloveOffset = _leftOffset;
        HandTracker.RightHand.GloveOffset = _rightOffset;
#endif
        var defaultRotation = HandTracker.GetDefaultRotation();
        _leftRotationOffset = SettingsManager.GetSetting(SettingsManager.LEFTGLOVEROTOFFSET, defaultRotation);
        _rightRotationOffset = SettingsManager.GetSetting(SettingsManager.RIGHTGLOVEROTOFFSET, defaultRotation);
#if UNITY_EDITOR && UNITY_ANDROID
        HandTracker.LeftEditorHand.GloveRotationOffset = _leftRotationOffset;
        HandTracker.RightEditorHand.GloveRotationOffset = _rightRotationOffset;
#else
        HandTracker.LeftHand.GloveRotationOffset = _leftRotationOffset;
        HandTracker.RightHand.GloveRotationOffset = _rightRotationOffset;
#endif
        SaveRequested = false;
    }
}