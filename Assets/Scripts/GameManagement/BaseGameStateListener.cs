using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameStateListener : MonoBehaviour
{
    
    private void OnEnable()
    {
        GameStateManager.Instance.gameStateChanged.AddListener(GameStateListener);
    }

    private void OnDisable()
    {
        GameStateManager.Instance.gameStateChanged.RemoveListener(GameStateListener);
    }

    protected abstract void GameStateListener(GameState oldState, GameState newState);
}
