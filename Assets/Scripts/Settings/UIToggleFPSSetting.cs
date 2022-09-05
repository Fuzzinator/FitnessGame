using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIToggleFPSSetting : UIToggleGroupSetting
{
    public override void Save()
    {
        SettingsManager.SetFPSSetting(_currentValue);
    }

    public override void Revert()
    {
        _currentValue = (int)SettingsManager.GetFPSSetting();
        SetActiveToggle();
    }
}
