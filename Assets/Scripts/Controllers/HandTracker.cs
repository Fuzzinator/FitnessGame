using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTracker : MonoBehaviour
{
    [SerializeField]
    private Hand _leftHand;
    [SerializeField]
    private Hand _rightHand;

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

    public static bool TryGetHand(Collider collider, out Hand hand)
    {
        hand = null;
        if (Instance == null)
        {
            Debug.LogError("Hand Tracker has not been initialized.");
            return false;
        }

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
        
        return false;
    }
}
