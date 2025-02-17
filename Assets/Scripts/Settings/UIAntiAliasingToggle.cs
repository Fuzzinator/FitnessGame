using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAntiAliasingToggle : UIToggleBoolSetting
{
    // Start is called before the first frame update
    void Start()
    {
        if(!PostProcessingManager.Instance.AllowAntiAliasing)
        {
            Destroy(gameObject);
            return;
        }
        _defaultValue = PostProcessingManager.Instance.AllowAntiAliasing;
    }

    public override void ToggleSet(bool isOn)
    {
        base.ToggleSet(isOn);
        UpdateAntiAliasing();
    }
    public override void Revert()
    {
        base.Revert();
        UpdateAntiAliasing();
    }

    private void UpdateAntiAliasing()
    {
        PostProcessingManager.Instance.UpdateAntiAliasing(_currentValue);
    }
}
