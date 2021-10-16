using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField]
    private InputActionAsset _mainInput;

    public InputActionAsset MainInput => _mainInput;

    [SerializeField]
    private InputActionAsset _menuInput;

    public InputActionAsset MenuInput => _menuInput;

    [SerializeField]
    private InputActionAsset _inGameInput;

    public InputActionAsset InGameInput => _inGameInput;

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

//        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        EnableActionMaps("UI Left Hand");
        EnableActionMaps("UI Right Hand");
    }

    public void EnableActionMaps(string actionName)
    {
        foreach (var action in _mainInput.actionMaps)
        {
            if (string.Equals(action.name,actionName)) 
            {
                action.Enable();
            }
        }
    }
    
}