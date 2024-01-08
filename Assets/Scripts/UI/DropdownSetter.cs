using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public abstract class DropdownSetter : MonoBehaviour
    {
        [SerializeField]
        protected TMP_Dropdown_XRSupport _dropdownField;

        private void Awake()
        {
            UpdateDropDownOptions();
        }

        public abstract void SetDropdownOption(int value);

        protected abstract void UpdateDropDownOptions();
    }
}