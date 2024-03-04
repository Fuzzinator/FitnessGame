using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIDefaultEnvironmentSetting : UIDropdownSetting
{
    protected override void OnEnable()
    {
        UpdateDropDownOptions();
        base.OnEnable();
        if (EnvironmentControlManager.Instance != null)
        {
            EnvironmentControlManager.Instance.availableReferencesUpdated.AddListener(UpdateDropDownOptions);
        }
    }

    private void UpdateDropDownOptions()
    {
        var listOfOptions = EnvironmentControlManager.Instance.GetNewAvailableEnvironmentsList();
        _dropdown.ClearOptions();
        _dropdown.AddOptions(listOfOptions);
        var index = GetSettingIndex();
    }

    private int GetSettingIndex()
    {
        if (string.IsNullOrWhiteSpace(_defaultValue))
        {
            _defaultValue = SettingsManager.GetSetting(_settingName, string.Empty);
            if (string.IsNullOrWhiteSpace(_defaultValue))
            {
                return 0;
            }
        }

        for (var i = 0; i < _dropdown.options.Count; i++)
        {
            var option = _dropdown.options[i];
            if (string.Equals(option.text, _defaultValue, StringComparison.InvariantCultureIgnoreCase))
            {
                return i;
            }
        }
        return 0;
    }
}
