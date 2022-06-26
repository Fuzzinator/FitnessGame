using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIToggleBoolSetting : MonoBehaviour, ISaver
{
   [SerializeField]
   private string _settingName;
   [SerializeField]
   private bool _defaultValue;
   
   [SerializeField]
   private Toggle _toggle;
   [SerializeField]
   private TextMeshProUGUI _text;
   [SerializeField]
   private SettingsDisplay _settingsDisplay;

   private const string ON = "On";
   private const string OFF = "Off";
   
   private void OnEnable()
   {
      var setting = SettingsManager.GetSetting(_settingName, _defaultValue);
      _toggle.isOn = setting;
      _text.SetText(setting?ON:OFF);
   }

   public void ToggleSet(bool isOn)
   {
      _text.SetText(isOn?ON:OFF);
      _settingsDisplay.ChangeWasMade(this);
   }

   public void Save()
   {
      SettingsManager.SetSetting(_settingName, _toggle.isOn);
   }

   public void Revert()
   {
      _toggle.isOn = SettingsManager.GetSetting(_settingName, _defaultValue);
   }
}
