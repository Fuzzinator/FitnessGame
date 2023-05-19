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
    protected bool _defaultValue;

    [SerializeField]
    private bool _cached = false;

    [SerializeField]
    private Toggle _toggle;

    [SerializeField]
    private TextMeshProUGUI _text;

    [SerializeField]
    private SettingsDisplay _settingsDisplay;

    [SerializeField]
    private ProfileEditor _profileEditor;

    protected bool _currentValue;

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

    public virtual void ToggleSet(bool isOn)
    {
        if (_currentValue != isOn)
        {
            _currentValue = isOn;
            _text.SetText(isOn ? ON : OFF);
            _settingsDisplay?.ChangeWasMade(this);

            SaveRequested = true;
        }
    }

    public virtual void Save(Profile overrideProfile = null)
    {
        if (_cached)
        {
            SettingsManager.SetCachedBool(_settingName, _currentValue, overrideProfile);
        }
        else
        {
            SettingsManager.SetSetting(_settingName, _currentValue, true, overrideProfile);
        }

        SaveRequested = false;
    }

    public virtual void Revert()
    {
        GetDefaultValue();
        _toggle.SetIsOnWithoutNotify(_currentValue);
        _text.SetText(_currentValue ? ON : OFF);
        SaveRequested = false;
    }

    protected void GetDefaultValue()
    {
        _currentValue = _cached
            ? SettingsManager.GetCachedBool(_settingName, _defaultValue, _profileEditor?.ActiveProfile)
            : SettingsManager.GetSetting(_settingName, _defaultValue, true, _profileEditor?.ActiveProfile);
    }
}