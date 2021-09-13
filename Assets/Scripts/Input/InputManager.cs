using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField]
    private InputActionMap _mainInput;

    public InputActionMap MainInput => _mainInput;

    [SerializeField]
    private InputActionMap _menuInput;

    public InputActionMap MenuInput => _menuInput;

    [SerializeField]
    private InputActionMap _inGameInput;

    public InputActionMap InGameInput => _inGameInput;

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

        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        if (_mainInput != null)
        {
            foreach (var action in _mainInput.actions)
            {
                action.Enable();
            }
        }
    }
}