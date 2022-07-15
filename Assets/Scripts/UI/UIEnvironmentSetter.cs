using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIEnvironmentSetter : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown _dropdownField;

    private void Start()
    {
        if (EnvironmentControlManager.Instance != null)
        {
            UpdateDropDownOptions();
        }
    }

    public void SetDropdownOption(int value)
    {
        if (EnvironmentControlManager.Instance != null)
        {
            EnvironmentControlManager.Instance.SetTargetEnvironmentIndex(value);
        }
    }

    private void UpdateDropDownOptions()
    {
        var listOfOptions = EnvironmentControlManager.Instance.GetNewAvailableEnvironmentsList();
        _dropdownField.ClearOptions();
        _dropdownField.AddOptions(listOfOptions);
        _dropdownField.value = 0;
    }
}