using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPostProcessingToggle : UIToggleBoolSetting
{
    private void Awake()
    {
        _defaultValue = PostProcessingManager.Instance.IsEnabledDefault;
    }

    public override void ToggleSet(bool isOn)
    {
        base.ToggleSet(isOn);
        UpdatePostProcessing();
    }
    public override void Revert()
    {
        base.Revert();
        UpdatePostProcessing();
    }

    private void UpdatePostProcessing()
    {
        PostProcessingManager.Instance.UpdateBloom(_currentValue);
    }
}
