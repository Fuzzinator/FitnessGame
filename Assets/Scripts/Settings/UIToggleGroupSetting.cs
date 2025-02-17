using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
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
    private ProfileEditor _profileEditor;

    [SerializeField]
    protected bool _cached = false;

    [SerializeField]
    protected int _defaultValue;

    [SerializeField]
    protected bool _setSettingOnEnable = false;
    
    protected int _currentValue;
    protected int _index;

    protected bool _updated;

    public int CurrentValue => _currentValue;

    public bool SaveRequested { get; set; }

    protected virtual void OnEnable()
    {
        DelayDisplayUpdateAsync(_setSettingOnEnable).Forget();
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
            _settingsDisplay?.ChangeWasMade(this);
            SaveRequested = true;
        }
    }

    public virtual void Save(Profile overrideProfile = null)
    {
        if (_cached)
        {
            SettingsManager.SetCachedInt(_settingName, _currentValue, overrideProfile);
        }
        else
        {
            SettingsManager.SetSetting(_settingName, _currentValue, true, overrideProfile);
        }
        
        SaveRequested = false;
    }

    protected async virtual UniTaskVoid DelayDisplayUpdateAsync(bool saveWhenDone = false)
    {
        _updated = true;
        await UniTask.DelayFrame(1);
        if(this == null)
        {
            return;
        }
        GetDefaultValue();
        var changedToggle = TrySetActiveToggle();
        
        SaveRequested = saveWhenDone;
    }

    public virtual void Revert()
    {
        GetDefaultValue();
        _updated = true;
        TrySetActiveToggle();
        SaveRequested = false;
    }

    protected bool TrySetActiveToggle()
    {
        if (_currentValue < _toggles.Length)
        {
            if(_toggles == null || _toggles[_currentValue] == null)
            {
                return false;
            }
            if(_toggles[_currentValue].isOn)
            {
                return false;
            }
            _toggles[_currentValue].isOn = true;
            return true;
        }
        return false;
    }

    protected bool TryDisableActiveToggle()
    {
        if (_currentValue < _toggles.Length)
        {
            if (_toggles == null || _toggles[_currentValue] == null)
            {
                return false;
            }
            if (!_toggles[_currentValue].isOn)
            {
                return false;
            }
            _toggles[_currentValue].isOn = false;
            return true;
        }
        return false;
    }

    protected void GetDefaultValue()
    {
        _currentValue = _cached
            ? SettingsManager.GetCachedInt(_settingName, _defaultValue, _profileEditor?.ActiveProfile)
            : SettingsManager.GetSetting(_settingName, _defaultValue, true, _profileEditor?.ActiveProfile);
    }
}