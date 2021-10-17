using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    /// <summary>
    /// Event fired when game state changes. First GameState is previous state. Second GameState is the new state.
    /// </summary>
    public UnityEvent<GameState, GameState> gameStateChanged = new UnityEvent<GameState, GameState>();

    [SerializeField]
    private GameState _gameState = GameState.Entry;
    private GameState _previousGameState = GameState.Entry;
    
    #region Const Strings
    private const string MENUBUTTON = "Menu Button";
#if UNITY_EDITOR
    private const string PAUSEINEDITOR = "Pause In Editor";
#endif
    #endregion
    
    public GameState CurrentGameState
    {
        get { return _gameState; }
        set
        {
            if (_gameState != value)
            {
                _previousGameState = _gameState;
                _gameState = value;
                gameStateChanged?.Invoke(_previousGameState, _gameState);
            }
        }
    }
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
    

    private void OnEnable()
    {
        if (InputManager.Instance != null && InputManager.Instance.MainInput != null)
        {
            if (InputManager.Instance != null && InputManager.Instance.MainInput != null)
            {   
                InputManager.Instance.MainInput[MENUBUTTON].performed += TryTogglePlayPauseState;
                FocusTracker.Instance.focusChanged.AddListener(ManageFocusState);
#if UNITY_EDITOR
                InputManager.Instance.MainInput[PAUSEINEDITOR].performed += TryTogglePlayPauseState;
#endif
            }
        }
    }
    private void OnDisable()
    {
        if (InputManager.Instance != null && InputManager.Instance.MainInput != null)
        {
            if (InputManager.Instance != null && InputManager.Instance.MainInput != null)
            {   
                InputManager.Instance.MainInput[MENUBUTTON].performed -= TryTogglePlayPauseState;
                FocusTracker.Instance.focusChanged.RemoveListener(ManageFocusState);
#if UNITY_EDITOR
                InputManager.Instance.MainInput[PAUSEINEDITOR].performed -= TryTogglePlayPauseState;
#endif
            }
        }
    }

    private void TryTogglePlayPauseState(InputAction.CallbackContext context)
    {
        if (_gameState != GameState.Paused && _gameState != GameState.Playing)
        {
            return;
        }

        CurrentGameState = _gameState switch
        {
            GameState.Playing => GameState.Paused,
            GameState.Paused => GameState.Playing,
            _ => CurrentGameState
        };
    }

    private void ManageFocusState(bool currentlyFocused)
    {
        if (!currentlyFocused)
        {
            CurrentGameState = GameState.Unfocused;
        } 
        else if (_gameState == GameState.Unfocused)
        {
            CurrentGameState = GameState.Paused;
        }
    }
}
[Serializable]
public enum GameState
{
    Entry = 0,
    InMainMenu = 1,
    Playing = 2,
    Paused = 3,
    Unfocused = 4
}
