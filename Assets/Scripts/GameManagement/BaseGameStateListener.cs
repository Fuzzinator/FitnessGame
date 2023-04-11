using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameStateListener : MonoBehaviour
{
    protected bool _useOnEnableDisable = true;
    protected virtual void OnEnable()
    {
        if(_useOnEnableDisable)
        {
            AddListener();
        }
    }

    protected virtual void OnDisable()
    {
        if(_useOnEnableDisable)
        {
            RemoveListener();
        }
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
