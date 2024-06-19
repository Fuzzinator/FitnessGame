using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
    private CancellationTokenSource _tokenSource;
    private CancellationToken _onDestroyToken;
    private bool _skipDelay = false;

    #region Const Strings
    private const string MenuButtonAll = "Menu Button All";
    private const string MenuButtonMeta = "Menu Button Quest";
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
                if (value == GameState.PreparingToPlay)
                {
                    TransitionToPlaying().Forget();
                }
                else
                {
                    _tokenSource.Cancel();
                    RefreshToken();
                }
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
                var menuInput = GetMenuInputName();
                InputManager.Instance.MainInput[menuInput].performed += TryTogglePlayPauseState;
                FocusTracker.Instance.OnFocusChanged.AddListener(ManageFocusState);
            }
        }
        _onDestroyToken = this.GetCancellationTokenOnDestroy();
        _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(_onDestroyToken);
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null && InputManager.Instance.MainInput != null)
        {
            if (InputManager.Instance != null && InputManager.Instance.MainInput != null)
            {
                var menuInput = GetMenuInputName();
                InputManager.Instance.MainInput[menuInput].performed -= TryTogglePlayPauseState;
                FocusTracker.Instance.OnFocusChanged.RemoveListener(ManageFocusState);
            }
        }
    }

    private void TryTogglePlayPauseState(InputAction.CallbackContext context)
    {
        if (_gameState != GameState.Paused && _gameState != GameState.Playing && _gameState != GameState.PreparingToPlay)
        {
            return;
        }

        switch (_gameState)
        {
            case GameState.Paused:
                CurrentGameState = GameState.PreparingToPlay;
                break;
            case GameState.Playing:
                CurrentGameState = GameState.Paused;
                break;
            case GameState.PreparingToPlay:
                CurrentGameState = GameState.Paused;
                break;
        }
    }

    private async UniTask TransitionToPlaying()
    {
        if (_skipDelay)
        {
            _skipDelay = false;
        }
        else
        {
            await UniTask.Delay(TimeSpan.FromSeconds(3), ignoreTimeScale: true, cancellationToken: _tokenSource.Token);
        }
        if (_tokenSource.IsCancellationRequested || _gameState != GameState.PreparingToPlay)
        {
            return;
        }
        CurrentGameState = GameState.Playing;
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

    private string GetMenuInputName()
    {
#if UNITY_ANDROID
        var headset = OVRPlugin.GetSystemHeadsetType();
        switch (headset)
        {
            case OVRPlugin.SystemHeadset.None:
                return MenuButtonAll;
            default: return MenuButtonMeta;
        }
#else
        return MenuButtonAll;
#endif
    }

    private void RefreshToken()
    {
        if (_tokenSource != null && !_onDestroyToken.IsCancellationRequested)
        {
            _tokenSource.Dispose();
            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(_onDestroyToken);
        }
    }

    public void SkipDelay()
    {
        _skipDelay = true;
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
