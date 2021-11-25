using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUIController : BaseGameStateListener
{
    public static MainMenuUIController Instance { get; private set; }

    [SerializeField]
    private Canvas _canvas;
    [SerializeField]
    private CanvasGroup[] _pages;
    
    private CanvasGroup _activeCanvasGroup;
    
    private List<MonoBehaviour> _requestSources = new List<MonoBehaviour>();

    private void Awake()
    {
        if (Instance == null || Instance.gameObject == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        _activeCanvasGroup = _pages[0];
        SetActivePage(0);
    }
    
    private void OnEnable()
    {
        UIStateManager.Instance.RequestEnableInteraction(_canvas);
    }

    private void OnDisable()
    {
        UIStateManager.Instance.RequestDisableInteraction(_canvas);
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
        if (gameObject == null)
        {
            return;
        }
        
        if (_activeCanvasGroup == null)
        {
            _activeCanvasGroup = _pages[0];
        }
        SetGroupState(_activeCanvasGroup, 1, true);
    }
    
    private void DisableUI()
    {
        if (gameObject == null)
        {
            return;
        }
        
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
