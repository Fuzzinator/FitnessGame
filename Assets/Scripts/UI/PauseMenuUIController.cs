using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUIController : BaseGameStateListener
{
    [SerializeField]
    private GameState _resumedState;
    
    [SerializeField]
    private Canvas _pauseMenuCanvas;

    protected override void GameStateListener(GameState oldState, GameState newState)
    {
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
    }

    private void DisableUI()
    {
        _pauseMenuCanvas.gameObject.SetActive(false);
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
        PlaylistManager.Instance.FullReset();
        ActiveSceneManager.Instance.LoadMainMenu();
    }
}