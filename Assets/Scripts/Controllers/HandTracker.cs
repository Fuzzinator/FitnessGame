using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTracker : MonoBehaviour, IOrderedInitialize
{
    public static HandTracker Instance { get; private set; }
    public bool Initialized { get; private set; }

    [SerializeField]
    private bool _spawnGloves = false;
    [SerializeField]
    private Hand _leftHand;
    [SerializeField]
    private Hand _rightHand;
#if UNITY_EDITOR

    [SerializeField]
    private Hand _leftEditorHand;
    [SerializeField]
    private Hand _rightEditorHand;

    public static Hand LeftEditorHand => Instance._leftEditorHand ?? Instance._leftHand;
    public static Hand RightEditorHand => Instance._rightEditorHand ?? Instance._leftHand;
#endif
    public static Hand LeftHand => Instance._leftHand;
    public static Hand RightHand => Instance._rightHand;

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

    public void Initialize()
    {
        if (Initialized)
        {
            return;
        }

        if (!_spawnGloves || EnvironmentControlManager.Instance == null)
        {
            return;
        }

        var dataContainer = EnvironmentControlManager.Instance.ActiveEnvironmentContainer;
        _leftHand.SetAndSpawnGlove(dataContainer.LeftGlove);
        _rightHand.SetAndSpawnGlove(dataContainer.RightGlove);
#if UNITY_EDITOR && UNITY_ANDROID
        _leftEditorHand.SetAndSpawnGlove(dataContainer.LeftGlove);
        _rightEditorHand.SetAndSpawnGlove(dataContainer.RightGlove);
#endif
        Initialized = true;
    }

    public static bool TryGetHand(Collider collider, out Hand hand)
    {
        hand = null;
        if (Instance == null)
        {
            Debug.LogError("Hand Tracker has not been initialized.");
            return false;
        }
#if UNITY_EDITOR && UNITY_ANDROID
        if (collider == LeftEditorHand.MyCollider)
        {
            hand = LeftEditorHand;
            return true;
        }
        if (collider == RightEditorHand.MyCollider)
        {
            hand = RightEditorHand;
            return true;
        }
#else
        if (collider == Instance._leftHand.MyCollider)
        {
            hand = Instance._leftHand;
            return true;
        }
        if (collider == Instance._rightHand.MyCollider)
        {
            hand = Instance._rightHand;
            return true;
        }
#endif
        return false;
    }

    public static Quaternion GetDefaultRotation()
    {
        var hasRightHand = RightHand != null;
        var defaultRotation = Quaternion.identity;
        if (hasRightHand)
        {
            defaultRotation = RightHand.DefaultRotation;
        }
        else
        {
            var hasLeftHand = LeftHand != null;
            if (hasLeftHand)
            {
                defaultRotation = LeftHand.DefaultRotation;
            }
        }
        return defaultRotation;
    }
}
