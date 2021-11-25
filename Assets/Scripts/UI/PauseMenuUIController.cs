using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUIController : BaseGameStateListener
{
    [SerializeField]
    private GameState _resumedState;
    
    [SerializeField]
    private Canvas _pauseMenuCanvas;

    [SerializeField]
    private bool _skipUI = false;
    
    protected override async void GameStateListener(GameState oldState, GameState newState)
    {
        if (_skipUI)
        {
            if (oldState == GameState.Unfocused && newState == GameState.Paused)
            {
                await UniTask.DelayFrame(1);
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
                DisableUI();
                break;
        }
    }

    private void EnableUI()
    {
        _pauseMenuCanvas.gameObject.SetActive(true);
        UIStateManager.Instance.RequestEnableInteraction(_pauseMenuCanvas);
    }

    private void DisableUI()
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
        LevelManager.Instance.LoadLevel();
        ResumeGame();
    }

    public void ReturnToMainMenu()
    {
        ResumeGame();
        PlaylistManager.Instance.FullReset();
        ActiveSceneManager.Instance.LoadMainMenu();
    }
}