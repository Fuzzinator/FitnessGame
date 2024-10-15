using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsDisplay : UIMenuController, IOrderedInitialize
{
    public static SettingsDisplay Instance { get; private set; }

    [SerializeField]
    private CanvasGroup _mainPages;

    [SerializeField]
    private CanvasGroup _popUpPage;

    [SerializeField]
    private CanvasGroup _confirmChangesPage;

    [SerializeField]
    private Button _saveButton;

    [SerializeField]
    private Button _revertButton;

    [SerializeField]
    private GameState _resumedState;

    private readonly List<ISaver> _activeSavers = new List<ISaver>();

    private static bool _changeMade;

    public bool ChangeMade
    {
        get => _changeMade;
        set
        {
            _saveButton.interactable = value;
            _revertButton.interactable = value;
            _changeMade = value;
        }
    }

    public bool Initialized { get; private set; }

    public void Initialize()
    {
        if(Initialized)
        {
            return;
        }

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    protected new void OnEnable()
    {
        AddListener();
    }

    protected new void OnDisable()
    {
        RemoveListener();
    }

    public override void Activate()
    {
        _mainPages.SetGroupState(true);
        base.Activate();
    }


    public void CheckForChangesAndDisable()
    {
        if (ChangeMade)
        {
            SetSaveScreen(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void ChangeWasMade(ISaver saver)
    {
        ChangeMade = true;
        if (_activeSavers.Contains(saver))
        {
            return;
        }

        _activeSavers.Add(saver);
    }


    public void DiscardChanges()
    {
        foreach (var saver in _activeSavers)
        {
            saver.Revert();
        }

        _activeSavers.Clear();
        ChangeMade = false;
    }

    public void SaveChanges()
    {
        foreach (var saver in _activeSavers)
        {
            saver.Save();
        }

        _activeSavers.Clear();
        ChangeMade = false;
    }

    public void SetPopUp(bool isOn)
    {
        _mainPages.SetGroupState(!isOn);

        _confirmChangesPage.SetGroupState(false);
        _confirmChangesPage.gameObject.SetActive(false);

        _popUpPage.SetGroupState(isOn);
        _popUpPage.gameObject.SetActive(isOn);
    }

    public void SetSaveScreen(bool isOn)
    {
        _mainPages.SetGroupState(!isOn);

        _popUpPage.SetGroupState(false);
        _popUpPage.gameObject.SetActive(false);

        _confirmChangesPage.SetGroupState(isOn);
        _confirmChangesPage.gameObject.SetActive(isOn);
    }


    protected void AddListener()
    {
        GameStateManager.Instance.gameStateChanged.AddListener(GameStateListener);
    }

    protected void RemoveListener()
    {
        GameStateManager.Instance.gameStateChanged.RemoveListener(GameStateListener);
    }

    protected void GameStateListener(GameState oldState, GameState newState)
    {
        switch (newState)
        {
            case GameState.PreparingToPlay:
            case GameState.Playing:
            case GameState.InMainMenu:
                if (oldState == GameState.Paused)
                {
                    DiscardChanges();
                    SetPopUp(false);
                    SetSaveScreen(false);
                    gameObject.SetActive(false);
                }

                break;
        }
    }

    public void ResumeGame()
    {
        GameStateManager.Instance.SetState(_resumedState);
    }
}