using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MainMenuUIController : BaseGameStateListener
{
    [SerializeField]
    private CanvasGroup _mainPage;

    [SerializeField]
    private CanvasGroup _doWorkoutPage;

    [SerializeField]
    private CanvasGroup _createWorkoutPage;

    private CanvasGroup _activeCanvasGroup;

    private CanvasGroup[] _allGroups;
    
    private void Start()
    {
        _activeCanvasGroup = _mainPage;
        _allGroups = new[] {_mainPage, _doWorkoutPage, _createWorkoutPage};
        SetActivePage(0);
    }

    public void SetActivePage(int targetPage)
    {
        switch (targetPage)
        {
            case 0:
                SetActivePage(_mainPage);
                break;
            case 1:
                SetActivePage(_doWorkoutPage);
                break;
            case 2:
                SetActivePage(_createWorkoutPage);
                break;
        }
    }

    private void SetActivePage(CanvasGroup targetGroup)
    {
        foreach (var group in _allGroups)
        {
            if (group != targetGroup)
            {
                SetGroupState(group, 0, false);
            }
            else
            {
                SetGroupState(group, 1, true);
                _activeCanvasGroup = group;
            }
        }
    }

    protected override void GameStateListener(GameState oldState, GameState newState)
    {
        switch (newState)
        {
            case GameState.Paused:
            case GameState.Unfocused:
                DisableUI();
                break;
            case GameState.Playing:
            case GameState.InMainMenu:
                EnableUI();
                break;
        }
    }
    
    private void EnableUI()
    {
        if (_activeCanvasGroup == null)
        {
            _activeCanvasGroup = _mainPage;
        }
        SetGroupState(_activeCanvasGroup, 1, true);
    }
    
    private void DisableUI()
    {
        if (_activeCanvasGroup == null)
        {
            _activeCanvasGroup = _mainPage;
        }
        SetGroupState(_activeCanvasGroup, .5f, false);
    }

    private void SetGroupState(CanvasGroup group, float alpha, bool state)
    {
        group.alpha = alpha;
        group.interactable = state;
        group.blocksRaycasts = state;
    }
}
