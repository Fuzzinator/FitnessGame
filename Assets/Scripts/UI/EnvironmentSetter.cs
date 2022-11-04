using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class EnvironmentSetter : DropdownSetter
    {
        private void OnEnable()
        {
            if (EnvironmentControlManager.Instance != null)
            {
                UpdateDropDownOptions();
                EnvironmentControlManager.Instance.availableReferencesUpdated.AddListener(UpdateDropDownOptions);
            }
        }

        private void OnDisable()
        {
            if (EnvironmentControlManager.Instance != null)
            {
                EnvironmentControlManager.Instance.availableReferencesUpdated.RemoveListener(UpdateDropDownOptions);
            }
        }

        public override void SetDropdownOption(int value)
        {
            if (EnvironmentControlManager.Instance != null)
            {
                EnvironmentControlManager.Instance.SetTargetEnvironmentIndex(value);
            }
        }

        protected override void UpdateDropDownOptions()
        {
            var listOfOptions = EnvironmentControlManager.Instance.GetNewAvailableEnvironmentsList();
            _dropdownField.ClearOptions();
            _dropdownField.AddOptions(listOfOptions);
            var index = GetOptionIndex(PlaylistManager.Instance.CurrentPlaylist.TargetEnvName);
            _dropdownField.SetValueWithoutNotify(index);
            SetDropdownOption(index);
        }

        protected virtual int GetOptionIndex(string optionName)
        {
            if (string.IsNullOrWhiteSpace(optionName))
            {
                return 0;
            }
            
            for (var i = 0; i < _dropdownField.options.Count; i++)
            {
                var option = _dropdownField.options[i];
                if (string.Equals(option.text, optionName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return i;
                }
            }
            return 0;
        }
    }
}