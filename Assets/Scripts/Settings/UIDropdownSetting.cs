using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDropdownSetting : MonoBehaviour, ISaver
{

    [SerializeField]
    private string _settingName;

    [SerializeField]
    protected string _defaultValue;

    [SerializeField]
    private bool _cached = false;

    [SerializeField]
    protected TMP_Dropdown_XRSupport _dropdown;

    [SerializeField]
    private SettingsDisplay _settingsDisplay;

    [SerializeField]
    private ProfileEditor _profileEditor;

    [SerializeField]
    protected bool _setSettingOnEnable = false;

    [SerializeField]
    protected bool _androidOnly;

    protected string _currentValue;

    public bool SaveRequested { get; set; }

    protected virtual void OnEnable()
    {
#if UNITY_STANDALONE_WIN
        if (_androidOnly)
        {
            Destroy(gameObject);
            return;
        }
#endif
        Revert();
        SaveRequested = _setSettingOnEnable;
    }

    protected virtual void OnDisable()
    {
        if (!SaveRequested)
        {
            Revert();
        }
    }

    public virtual void DropdownSet(int index)
    {
        DropdownSet(index, true);
    }

    protected virtual void DropdownSet(int index, bool onlyIfDifferent)
    {
        if (index >= _dropdown.options.Count)
        {
            return;
        }
        if (!onlyIfDifferent || _currentValue != _dropdown.options[index].text)
        {
            _currentValue = _dropdown.options[index].text;
            _settingsDisplay?.ChangeWasMade(this);

            SaveRequested = true;
        }
    }

    public virtual void Save(Profile overrideProfile = null)
    {
        SettingsManager.SetSetting(_settingName, _currentValue, true, overrideProfile);

        SaveRequested = false;
    }

    public virtual void Revert()
    {
        GetDefaultValue();
        _dropdown.SetValueWithoutNotify(GetIndexFromText(_currentValue));

        if (_setSettingOnEnable)
        {
            DropdownSet(GetIndexFromText(_currentValue), false);
        }
        else
        {
            SaveRequested = false;
        }
    }

    protected void GetDefaultValue()
    {
        _currentValue = SettingsManager.GetSetting(_settingName, _defaultValue, true, _profileEditor?.ActiveProfile);
    }

    protected int GetIndexFromText(string text)
    {
        for (var i = 0; i < _dropdown.options.Count; i++)
        {
            var option = _dropdown.options[i];
            if (string.Equals(option.text, text))
            {
                return i;
            }
        }
        return 0;
    }
}
