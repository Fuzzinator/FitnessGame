using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIToggleGroupSetting : MonoBehaviour, ISaver
{
    [SerializeField]
    private string _settingName;

    [SerializeField]
    protected Toggle[] _toggles;

    [SerializeField]
    protected SettingsDisplay _settingsDisplay;
    
    [SerializeField]
    protected bool _cached = false;

    [SerializeField]
    protected int _defaultValue;
    
    protected int _currentValue;
    protected int _index;

    public int CurrentValue => _currentValue;

    public bool SaveRequested { get; set; }

    private void OnEnable()
    {
        Revert();
        SaveRequested = false;
    }

    public virtual void ToggleSet(Toggle selectedToggle)
    {
        if (!selectedToggle.isOn)
        {
            return;
        }

        _index = -1;
        for (var i = 0; i < _toggles.Length; i++)
        {
            if (_toggles[i] == selectedToggle)
            {
                _index = i;
                break;
            }
        }

        if (_currentValue != _index)
        {
            _currentValue = _index;
            _settingsDisplay.ChangeWasMade(this);
            SaveRequested = true;
        }
    }

    public virtual void Save()
    {
        if (_cached)
        {
            SettingsManager.SetCachedInt(_settingName, _currentValue);
        }
        else
        {
            SettingsManager.SetSetting(_settingName, _currentValue);
        }
        
        SaveRequested = false;
    }

    public virtual void Revert()
    {
        GetDefaultValue();
        SetActiveToggle();
        SaveRequested = false;
    }

    protected void SetActiveToggle()
    {
        if (_currentValue < _toggles.Length)
        {
            _toggles[_currentValue].isOn = true;
        }
    }

    protected void GetDefaultValue()
    {
        _currentValue = _cached
            ? SettingsManager.GetCachedInt(_settingName, _defaultValue)
            : SettingsManager.GetSetting(_settingName, _defaultValue);
    }
}