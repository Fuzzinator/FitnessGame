using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MainMenuUIController : BaseGameStateListener
{
    [SerializeField]
    private CanvasGroup[] _pages;
    
    private CanvasGroup _activeCanvasGroup;
    
    private List<MonoBehaviour> _requestSources = new List<MonoBehaviour>();
    
    private void Start()
    {
        _activeCanvasGroup = _pages[0];
        SetActivePage(0);
    }

    public void SetActivePage(int targetPage)
    {
        SetActivePage(_pages[targetPage]);
    }

    private void SetActivePage(CanvasGroup targetGroup)
    {
        foreach (var group in _pages)
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
                RequestDisableUI(this);
                break;
            case GameState.Playing:
            case GameState.InMainMenu:
                RequestEnableUI(this);
                break;
        }
    }

    public void RequestDisableUI(MonoBehaviour behaviour)
    {
        if (_requestSources.Contains(behaviour))
        {
            return;
        }
        _requestSources.Add(behaviour);
        DisableUI();
    }

    public void RequestEnableUI(MonoBehaviour behaviour)
    {
        if (!_requestSources.Contains(behaviour))
        {
            return;
        }

        _requestSources.Remove(behaviour);
        if (_requestSources.Count == 0)
        {
            EnableUI();
        }
    }
    
    private void EnableUI()
    {
        if (_activeCanvasGroup == null)
        {
            _activeCanvasGroup = _pages[0];
        }
        SetGroupState(_activeCanvasGroup, 1, true);
    }
    
    private void DisableUI()
    {
        if (_activeCanvasGroup == null)
        {
            _activeCanvasGroup = _pages[0];
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
