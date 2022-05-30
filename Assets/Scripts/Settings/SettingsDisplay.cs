using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsDisplay : MonoBehaviour
{
    public static SettingsDisplay Instance { get; private set; }
    [SerializeField]
    private List<Toggle> _toggles;

    [SerializeField]
    private CanvasGroup[] _settingsPages;

    [SerializeField]
    private CanvasGroup _mainPages;

    [SerializeField]
    private CanvasGroup _popUpPage;
    
    [SerializeField]
    private CanvasGroup _confirmChangesPage;
    
    [SerializeField]
    private Button _saveButton;
    
    private CanvasGroup _activePage;

    private Toggle _activeToggle;

    private readonly List<ISaver> _activeSavers = new List<ISaver>();
    
    private static bool _changeMade;

    public bool ChangeMade
    {
        get => _changeMade;
        set
        {
            _saveButton.interactable = value;
            _changeMade = value;
        }
    }

    private void Awake()
    {
        if (Instance == null)
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
        if (_settingsPages == null || _settingsPages.Length < 1)
        {
            return;
        }
        
        _activePage = _settingsPages[0];
        _activePage.SetGroupState(1, true);
        var canvas = GetComponent<Canvas>();
        canvas.worldCamera = Head.Instance.HeadCamera;
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        _mainPages.SetGroupState(true);
        for (var i = 0; i < _toggles.Count; i++)
        {
            _toggles[i].isOn = i == 0;
        }
        SetActivePage(0);
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

    public void PageToggleChanged(Toggle toggle)
    {
        if (!toggle.isOn)
        {
            return;
        }

        _activeToggle = toggle;
        SetActivePage(_toggles.IndexOf(toggle));
    }

    public void SetActivePage(int pageNumber)
    {
        if (pageNumber >= _settingsPages.Length)
        {
            return;
        }

        if (_activePage != null)
        {
            _activePage.SetGroupState(0, false);
        }

        _activePage = _settingsPages[pageNumber];
        _activePage.SetGroupState(1, true);
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
}