using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIToggleFPSSetting : MonoBehaviour, ISaver
{
    [SerializeField]
    private Toggle[] _toggles;

    [SerializeField]
    private SettingsDisplay _settingsDisplay;

    private int _currentValue;

    private void OnEnable()
    {
        Revert();
    }

    public void ToggleSet(Toggle selectedToggle)
    {
        if (!selectedToggle.isOn)
        {
            return;
        }
        
        var index = -1;
        for (var i = 0; i < _toggles.Length; i++)
        {
            if (_toggles[i] == selectedToggle)
            {
                index = i;
                break;
            }
        }

        if (_currentValue != index)
        {
            _currentValue = index;
            _settingsDisplay.ChangeWasMade(this);
        }
    }

    public void Save()
    {
        SettingsManager.SetFPSSetting(_currentValue);
    }

    public void Revert()
    {
        _currentValue = (int)SettingsManager.GetFPSSetting();
        if (_currentValue < _toggles.Length)
        {
            _toggles[_currentValue].isOn = true;
        }
    }
}
