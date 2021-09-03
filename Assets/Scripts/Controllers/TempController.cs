using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TempController : MonoBehaviour
{
    [SerializeField]
    private InputActionReference _button;
    private Action<InputAction.CallbackContext> _actionSubscription;

    [SerializeField]
    private UnityEvent<InputAction.CallbackContext> pressedButton;
    // Start is called before the first frame update
    void Start()
    {
        _actionSubscription = (context) => pressedButton?.Invoke(context);
        _button.action.started += _actionSubscription;
    }

    private void OnDestroy()
    {
        _button.action.started -= _actionSubscription;
    }
}
