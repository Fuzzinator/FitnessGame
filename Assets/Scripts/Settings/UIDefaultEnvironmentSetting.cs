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
    }
}
