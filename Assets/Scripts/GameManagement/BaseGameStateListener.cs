using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameStateListener : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        AddListener();
    }

    protected virtual void OnDisable()
    {
        RemoveListener();
    }

    protected virtual void AddListener()
    {
        GameStateManager.Instance.gameStateChanged.AddListener(GameStateListener);
    }

    protected virtual void RemoveListener()
    {
        GameStateManager.Instance.gameStateChanged.RemoveListener(GameStateListener);
    }

    protected abstract void GameStateListener(GameState oldState, GameState newState);
}
