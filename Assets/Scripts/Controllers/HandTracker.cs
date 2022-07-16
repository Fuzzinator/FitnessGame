using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTracker : MonoBehaviour
{
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
    
    public static Hand LeftEditorHand => Instance._leftEditorHand?? Instance._leftHand;
    public static Hand RightEditorHand => Instance._rightEditorHand??Instance._leftHand;
    #endif
    public static Hand LeftHand => Instance._leftHand;
    public static Hand RightHand => Instance._rightHand;
    public static HandTracker Instance { get; private set; }

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

    private void Start()
    {
        if (!_spawnGloves || EnvironmentControlManager.Instance == null)
        {
            return;
        }
        
        var dataContainer = EnvironmentControlManager.Instance.ActiveEnvironmentContainer;
        _leftHand.SetAndSpawnGlove(dataContainer.LeftGlove);
        _rightHand.SetAndSpawnGlove(dataContainer.RightGlove);
        #if UNITY_EDITOR
        _leftEditorHand.SetAndSpawnGlove(dataContainer.LeftGlove);
        _rightEditorHand.SetAndSpawnGlove(dataContainer.RightGlove);
        #endif
    }
    
    public static bool TryGetHand(Collider collider, out Hand hand)
    {
        hand = null;
        if (Instance == null)
        {
            Debug.LogError("Hand Tracker has not been initialized.");
            return false;
        }
        #if UNITY_EDITOR
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
    
    
}
