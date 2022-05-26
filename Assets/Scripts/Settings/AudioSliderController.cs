using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioSliderController : MonoBehaviour, ISaver
{
    [SerializeField]
    private SettingsDisplay _settingsDisplay;

    [SerializeField]
    private TextMeshProUGUI _currentText;

    [SerializeField]
    private VolumeMixer _mixerType;

    [SerializeField]
    private Slider _slider;

    private float _setVolume;

    #region Const Strings

    private const string PERCENT = "%";

    #endregion

    private void Start()
    {
        _setVolume = SettingsManager.Instance.GetVolumeMixer(_mixerType);
    }

    private void OnEnable()
    {
        _slider.value = SettingsManager.Instance.GetVolumeMixer(_mixerType);
    }

    public void ApplyChange()
    {
        SettingsManager.Instance.SetVolumeMixer(_mixerType, _slider.value);
        _settingsDisplay.ChangeWasMade(this);

    }

    public void Save()
    {
        SettingsManager.SetSetting(SettingsManager.GetVolumeMixerName(_mixerType), _slider.value);
    }

    public void Revert()
    {
        _slider.value = SettingsManager.GetSetting(SettingsManager.GetVolumeMixerName(_mixerType), 1f);
        
        SettingsManager.Instance.SetVolumeMixer(_mixerType, _slider.value);
    }

public void OnChangeSlider(float value)
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.Append((int)(value*100));
            sb.Append(PERCENT);
            
            _currentText.SetText(sb);
        }
    }
}
