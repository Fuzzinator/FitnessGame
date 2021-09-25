using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private InputManager _inputManager;

    public bool GameIsPaused { get; private set; }

    #region Const Strings

    private const string MENUBUTTON = "Menu Button";
#if UNITY_EDITOR
    private const string PAUSEINEDITOR = "Pause In Editor";
#endif

    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(Instance);
        }
    }

    private void OnEnable()
    {
        _inputManager.MainInput[MENUBUTTON].performed += ToggleGamePauseState;
#if UNITY_EDITOR
        _inputManager.MainInput[PAUSEINEDITOR].performed += ToggleGamePauseState;
#endif
    }

    public void ToggleGamePauseState(InputAction.CallbackContext callbackContext)
    {
        if (GameIsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        GameIsPaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        GameIsPaused = false;
    }
}