using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIToggleBoolSetting : MonoBehaviour, ISaver
{
    [SerializeField]
    private string _settingName;

    [SerializeField]
    private bool _defaultValue;

    [SerializeField]
    private bool _cached = false;

    [SerializeField]
    private Toggle _toggle;

    [SerializeField]
    private TextMeshProUGUI _text;

    [SerializeField]
    private SettingsDisplay _settingsDisplay;

    private bool _currentValue;

    public bool SaveRequested { get; set; }

    private const string ON = "On";
    private const string OFF = "Off";

    private void OnEnable()
    {
        Revert();
        SaveRequested = false;
    }

    private void OnDisable()
    {
        if (!SaveRequested)
        {
            Revert();
        }
    }

    public void ToggleSet(bool isOn)
    {
        if (_currentValue != isOn)
        {
            _currentValue = isOn;
            _text.SetText(isOn ? ON : OFF);
            _settingsDisplay.ChangeWasMade(this);

            SaveRequested = true;
        }
    }

    public void Save(Profile overrideProfile = null)
    {
        if (_cached)
        {
            SettingsManager.SetCachedBool(_settingName, _currentValue);
        }
        else
        {
            SettingsManager.SetSetting(_settingName, _currentValue);
        }

        SaveRequested = false;
    }

    public void Revert()
    {
        _currentValue = _cached
            ? SettingsManager.GetCachedBool(_settingName, _defaultValue)
            : SettingsManager.GetSetting(_settingName, _defaultValue);
        
        _toggle.SetIsOnWithoutNotify(_currentValue);
        _text.SetText(_currentValue ? ON : OFF);
        SaveRequested = false;
    }
}