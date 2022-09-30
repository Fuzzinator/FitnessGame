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
            _dropdownField.value = 0;
        }
    }
}