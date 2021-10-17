using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetGameState : MonoBehaviour
{
    [SerializeField]
    private GameState _stateToSet;

    private void Start()
    {
        GameStateManager.Instance.CurrentGameState = _stateToSet;
    }
}
