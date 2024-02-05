using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISliderFloatSetting : MonoBehaviour, ISaver
{
    [SerializeField]
    protected string _settingName;

    [SerializeField]
    protected float _defaultValue;

    [SerializeField]
    protected bool _cached = false;

    [SerializeField]
    protected Slider _slider;

    [SerializeField]
    protected TextMeshProUGUI _currentText;

    [SerializeField]
    protected SettingsDisplay _settingsDisplay;

    [SerializeField]
    protected ProfileEditor _profileEditor;

    public bool SaveRequested { get; set; }

    protected float _currentValue;

    #region Const Strings
    private const string Format = "{0:.##}";
    #endregion

    protected virtual void OnEnable()
    {
        Revert();
        SaveRequested = false;
    }

    protected virtual void OnDisable()
    {
        if (!SaveRequested)
        {
            Revert();
        }
    }

    public virtual void ApplyChange()
    {
        SetValue();
        _settingsDisplay.ChangeWasMade(this);
        SaveRequested = true;
    }

    protected virtual void SetValue()
    {
        _currentValue = _slider.value;
    }

    public virtual void Save(Profile overrideProfile = null)
    {
        if (_cached)
        {
            SettingsManager.SetCachedFloat(_settingName, _slider.value);
        }
        else
        {
            SettingsManager.SetSetting(_settingName, _slider.value);
        }
        SaveRequested = false;
    }

    public virtual void Revert()
    {
        GetDefaultValue();
        _slider.SetValueWithoutNotify(_currentValue);
        OnChangeSlider(_currentValue);
        SaveRequested = false;
    }
    protected virtual void GetDefaultValue()
    {
        _currentValue = _cached
            ? SettingsManager.GetCachedFloat(_settingName, _defaultValue, _profileEditor?.ActiveProfile)
            : SettingsManager.GetSetting(_settingName, _defaultValue, true, _profileEditor?.ActiveProfile);
    }

    public virtual void OnChangeSlider(float value)
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            var frac = value - (int)value;
            if (frac < 0.05f || frac > 0.95f)
            {

            }
            value = (float)Mathf.Round(value / .05f) * .05f;

            sb.AppendFormat(Format, value);

            _currentText.SetText(sb);
        }
    }
}
