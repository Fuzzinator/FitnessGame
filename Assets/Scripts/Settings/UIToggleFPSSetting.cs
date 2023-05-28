using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class UIToggleFPSSetting : UIToggleGroupSetting
{
    public override void Save(Profile overrideProfile = null)
    {
        SettingsManager.SetFPSSetting(_currentValue);
    }

    public override void Revert()
    {
        _currentValue = (int)SettingsManager.GetFPSSetting();
        TrySetActiveToggle();
    }

    protected override async UniTaskVoid DelayDisplayUpdateAsync()
    {
        _updated = true;
        await UniTask.DelayFrame(1);
        if (this == null)
        {
            return;
        }
        Revert();
        SaveRequested = false;
    }
}
