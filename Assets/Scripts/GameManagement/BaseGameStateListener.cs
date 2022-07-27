using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameStateListener : MonoBehaviour
{
    
    private void OnEnable()
    {
        AddListener();
    }

    private void OnDisable()
    {
        RemoveListener();
    }

    protected void AddListener()
    {
        GameStateManager.Instance.gameStateChanged.AddListener(GameStateListener);
    }

    protected void RemoveListener()
    {
        GameStateManager.Instance.gameStateChanged.RemoveListener(GameStateListener);
    }

    protected abstract void GameStateListener(GameState oldState, GameState newState);
}
