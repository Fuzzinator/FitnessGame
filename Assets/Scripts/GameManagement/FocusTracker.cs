using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FocusTracker : MonoBehaviour
{
    public static FocusTracker Instance { get; private set; }

    [field: SerializeField]
    public UnityEvent<bool> OnFocusChanged { get; private set; } = new UnityEvent<bool>();

    public bool IsFocused
    {
        get;
        private set;
    }

#if UNITY_EDITOR
    [Header("Editor Only Properties"), SerializeField]
    private bool _trackFocus = true;
#endif
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

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
#if UNITY_ANDROID
    private void OnApplicationFocus(bool hasFocus)
    {
#if UNITY_EDITOR
        if (_trackFocus)
#endif
            OnFocusChanged?.Invoke(hasFocus);
        IsFocused = hasFocus;
    }

#else
    private void OnApplicationPause(bool pause)
    {
#if UNITY_EDITOR
        if (_trackFocus)
#endif
            OnFocusChanged?.Invoke(pause);
        IsFocused = pause;
    }
#endif
}