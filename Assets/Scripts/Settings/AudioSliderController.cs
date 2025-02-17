using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioSliderController : UISliderFloatSetting, ISaver
{
    [SerializeField]
    protected VolumeMixer _mixerType;
    #region Const Strings

    private const string PERCENT = "%";

    #endregion

    protected override void OnEnable()
    {
        _slider.SetValueWithoutNotify(SettingsManager.Instance.GetVolumeMixer(_mixerType));
        
        SaveRequested = false;
    }

    protected override void OnDisable()
    {
        if (!SaveRequested)
        {
            Revert();
        }
    }

    protected override void SetValue()
    {
        SettingsManager.Instance.SetVolumeMixer(_mixerType, _slider.value);
    }

    public override void Save(Profile overrideProfile = null)
    {
        SettingsManager.SetSetting(SettingsManager.GetVolumeMixerName(_mixerType), _slider.value);
        SaveRequested = false;
    }

    public override void Revert()
    {
        _slider.value = SettingsManager.GetSetting(SettingsManager.GetVolumeMixerName(_mixerType), 1f);
        
        SettingsManager.Instance.SetVolumeMixer(_mixerType, _slider.value);
        SaveRequested = false;
    }

public override void OnChangeSlider(float value)
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.Append((int)(value*100));
            sb.Append(PERCENT);
            
            _currentText.SetText(sb);
        }
    }
}
