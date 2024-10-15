using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUIController : BaseGameStateListener, IOrderedInitialize
{
    [SerializeField]
    private GameState _resumedState;

    [SerializeField]
    private Canvas _pauseMenuCanvas;

    [SerializeField]
    private bool _skipUI = false;

    [SerializeField]
    private TransitionController _transitionController;

    public bool Initialized { get; private set; }

    public void Initialize()
    {
        AddListener();
    }

    protected override void OnEnable() { }

    protected override void OnDisable() { }

    private void OnDestroy()
    {
        DisableUI();
        RemoveListener();
    }

    protected override async void GameStateListener(GameState oldState, GameState newState)
    {
        if (_skipUI || !gameObject.activeInHierarchy)
        {
            if ((oldState == GameState.Unfocused && newState == GameState.Paused) || (oldState == GameState.Paused && newState == GameState.PreparingToPlay))
            {
                await UniTask.DelayFrame(1);
                GameStateManager.Instance.SkipDelay();
                ResumeGame();
            }

            return;
        }

        switch (newState)
        {
            case GameState.Paused:
            case GameState.Unfocused:
                EnableUI();
                break;
            case GameState.Playing:
            case GameState.InMainMenu:
            case GameState.PreparingToPlay:
                DisableUI();
                break;
        }
    }

    public void SetSkipUI(bool skip)
    {
        _skipUI = skip;
    }

    public void EnableUI()
    {
        _pauseMenuCanvas.gameObject.SetActive(true);
        UIStateManager.Instance.RequestEnableInteraction(_pauseMenuCanvas);
    }

    public void DisableUI()
    {
        _pauseMenuCanvas.gameObject.SetActive(false);
        UIStateManager.Instance.RequestDisableInteraction(_pauseMenuCanvas);
    }

    public void ResumeGame()
    {
        GameStateManager.Instance.SetState(_resumedState);
    }

    public void Restart()
    {
        LevelManager.Instance.Restart();
        ResumeGame();
    }

    public void StartReturnToMainMenu()
    {
        GameStateManager.Instance.SetState(GameState.Playing);
        PlaylistManager.Instance.FullReset();
        _transitionController.RequestTransition();
    }

    public void ReturnToMainMenu()
    {
        ActiveSceneManager.Instance.LoadMainMenu();
    }
}