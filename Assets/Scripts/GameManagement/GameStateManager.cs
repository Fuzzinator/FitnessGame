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
    #endregion
    
    public GameState CurrentGameState
    {
        get { return _gameState; }
        private set
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

    public void SetState(GameState state)
    {
        
        CurrentGameState = state;
    }
}
[Serializable]
public enum GameState
{
    Entry = 0,
    InMainMenu = 1,
    Playing = 2,
    Paused = 3,
    Unfocused = 4,
    MenuToGameTransition = 5,
    PreparingToPlay = 6,
    TransitionBetweenSongs = 7,
    GameToMenuTransition = 8,
}
