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
    private Vector3 _rightStartPos;

    private Vector3 _leftBeginPos;
    private Vector3 _rightBeginPos;
    
    private Vector3 _leftOffset;
    private Vector3 _rightOffset;

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
    }
    private void RightGripReleased(InputAction.CallbackContext obj)
    {
        _rightGripPressed = false;
        HandTracker.RightHand.ParentGlove();
        _rightOffset = HandTracker.RightHand.GloveOffset;
    }

    private void TrackControllerPosition(Hand hand)
    {
        var leftHand = hand.AssignedHand == HitSideType.Left;
        if (leftHand)
        {
            _leftBeginPos = hand.transform.position;
        }
        else
        {
            _rightBeginPos = hand.transform.position;
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
    }

    public void Revert()
    {
        _leftOffset = SettingsManager.GetSetting(SettingsManager.LEFTGLOVEOFFSET, Vector3.zero); 
        _rightOffset = SettingsManager.GetSetting(SettingsManager.RIGHTGLOVEOFFSET, Vector3.zero);
        HandTracker.LeftHand.GloveOffset = _leftOffset;
        HandTracker.RightHand.GloveOffset = _rightOffset;
    }
}
