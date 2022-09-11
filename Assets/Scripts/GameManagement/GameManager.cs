using System;
using System.Collections;
using System.Collections.Generic;
using GameModeManagement;
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

    private void OnEnable()
    {
        GameStateManager.Instance.gameStateChanged.AddListener(HandleGameStateChange);
    }

    private void HandleGameStateChange(GameState oldState, GameState newState)
    {
        if ((newState == GameState.Paused || newState == GameState.Unfocused) && oldState != GameState.Paused)
        {
            PauseGame();
        }
        else if ((newState == GameState.Playing || newState == GameState.InMainMenu) &&
                 (oldState == GameState.Paused || oldState == GameState.Unfocused))
        {
            ResumeGame();
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