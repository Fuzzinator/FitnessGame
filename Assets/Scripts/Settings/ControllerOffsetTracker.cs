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
        
        
        _leftStartPos = HandTracker.LeftHand.GloveOffset;
        _rightStartPos = HandTracker.RightHand.GloveOffset;
    }


    private void OnDisable()
    {
        InputManager.Instance.MainInput[LEFTGRIPPRESSED].performed -= LeftGripPressed;
        InputManager.Instance.MainInput[RIGHTGRIPPRESSED].performed -= RightGripPressed;
        InputManager.Instance.MainInput[LEFTGRIPRELEASED].performed -= LeftGripReleased;
        InputManager.Instance.MainInput[RIGHTGRIPRELEASED].performed -= RightGripReleased;
    }

    private void LeftGripPressed(InputAction.CallbackContext obj)
    {
        _leftGripPressed = true;
        TrackControllerPosition(HandTracker.LeftHand);
    }
    private void RightGripPressed(InputAction.CallbackContext obj)
    {
        _rightGripPressed = true;
        TrackControllerPosition(HandTracker.RightHand);
    }
    private void LeftGripReleased(InputAction.CallbackContext obj)
    {
        _leftGripPressed = false;
        HandTracker.LeftHand.ParentGlove();
        _leftOffset = HandTracker.LeftHand.GloveOffset;
        _leftRotationOffset = HandTracker.LeftHand.GloveRotationOffset;
    }
    private void RightGripReleased(InputAction.CallbackContext obj)
    {
        _rightGripPressed = false;
        HandTracker.RightHand.ParentGlove();
        _rightOffset = HandTracker.RightHand.GloveOffset;
        _rightRotationOffset = HandTracker.RightHand.GloveRotationOffset;
    }

    private void TrackControllerPosition(Hand hand)
    {
        var leftHand = hand.AssignedHand == HitSideType.Left;
        var handTransform = hand.transform;
        if (leftHand)
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
        SettingsDisplay.Instance.ChangeWasMade(this);
    }

    public void CancelChanges()
    {
        HandTracker.LeftHand.GloveOffset = _leftStartPos;
        HandTracker.RightHand.GloveOffset = _rightStartPos;
    }

    public void Save()
    {
        SettingsManager.SetSetting(SettingsManager.LEFTGLOVEOFFSET, _leftOffset);
        SettingsManager.SetSetting(SettingsManager.RIGHTGLOVEOFFSET, _rightOffset);
        SettingsManager.SetSetting(SettingsManager.LEFTGLOVEROTOFFSET, _leftOffset);
        SettingsManager.SetSetting(SettingsManager.RIGHTGLOVEROTOFFSET, _rightOffset);
    }

    public void ResetLeftController()
    {
        ResetController(HandTracker.LeftHand);
    }

    public void ResetRightController()
    {
        ResetController(HandTracker.RightHand);
    }

    private void ResetController(Hand hand)
    {
        var leftHand = hand.AssignedHand == HitSideType.Left;
        
        hand.GloveOffset = Vector3.zero;
        hand.GloveRotationOffset = Quaternion.identity;
        
        var handTransform = hand.transform;
        if (leftHand)
        {
            _leftOffset = Vector3.zero;
            _leftRotationOffset = Quaternion.identity;
            SettingsManager.SetSetting(SettingsManager.LEFTGLOVEOFFSET, _leftOffset);
            SettingsManager.SetSetting(SettingsManager.LEFTGLOVEROTOFFSET, _leftRotationOffset);
        }
        else
        {
            _rightOffset = Vector3.zero;
            _rightRotationOffset = Quaternion.identity;
            SettingsManager.SetSetting(SettingsManager.RIGHTGLOVEOFFSET, _rightOffset);
            SettingsManager.SetSetting(SettingsManager.RIGHTGLOVEROTOFFSET, _rightRotationOffset);
        }
    }
    
    public void Revert()
    {
        _leftOffset = SettingsManager.GetSetting(SettingsManager.LEFTGLOVEOFFSET, Vector3.zero); 
        _rightOffset = SettingsManager.GetSetting(SettingsManager.RIGHTGLOVEOFFSET, Vector3.zero);
        HandTracker.LeftHand.GloveOffset = _leftOffset;
        HandTracker.RightHand.GloveOffset = _rightOffset;

        _leftRotationOffset = SettingsManager.GetSetting(SettingsManager.LEFTGLOVEROTOFFSET, Quaternion.identity);
        _rightRotationOffset = SettingsManager.GetSetting(SettingsManager.RIGHTGLOVEROTOFFSET, Quaternion.identity);
        HandTracker.LeftHand.GloveRotationOffset = _leftRotationOffset;
        HandTracker.RightHand.GloveRotationOffset = _rightRotationOffset;
    }
}
