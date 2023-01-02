using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownResetter : MonoBehaviour
{
    [SerializeField]
    private int _default;

    [SerializeField]
    private TMP_Dropdown _dropdown;
    private void OnEnable()
    {
        Reset();
    }

    public void Reset()
    {
        if (_default >= _dropdown.options.Count)
        {
            return;
        }
        
        _dropdown.SetValueWithoutNotify(_default);
    }
}
