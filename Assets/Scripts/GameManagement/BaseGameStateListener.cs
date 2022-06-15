using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameStateListener : MonoBehaviour
{
    
    protected virtual void OnEnable()
    {
        GameStateManager.Instance.gameStateChanged.AddListener(GameStateListener);
    }

    protected virtual void OnDisable()
    {
        GameStateManager.Instance.gameStateChanged.RemoveListener(GameStateListener);
    }

    protected abstract void GameStateListener(GameState oldState, GameState newState);
}
