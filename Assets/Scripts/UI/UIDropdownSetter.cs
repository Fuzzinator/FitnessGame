using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class UIDropdownSetter : MonoBehaviour
{
    [SerializeField]
    protected TMP_Dropdown _dropdownField;

    private void Awake()
    {
        UpdateDropDownOptions();
    }

    public abstract void SetDropdownOption(int value);
    
    protected abstract void UpdateDropDownOptions();
}
