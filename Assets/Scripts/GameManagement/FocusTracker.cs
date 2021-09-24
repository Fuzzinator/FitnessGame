using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FocusTracker : MonoBehaviour
{
    public static FocusTracker Instance { get; private set; }
    
    public UnityEvent<bool> focusChanged = new UnityEvent<bool>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        focusChanged?.Invoke(hasFocus);
    }
}
