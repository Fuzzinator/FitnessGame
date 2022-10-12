using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class NewPlaylistDifficultySetter : DropdownSetter
    {
        private void OnEnable()
        {
            if (PlaylistMaker.Instance == null)
            {
                return;
            }

            UpdateDisplayedValues();
        }
        
        public override void SetDropdownOption(int value)
        {
            if (PlaylistMaker.Instance == null)
            {
                return;
            }

            PlaylistMaker.Instance.SetDifficulty((DifficultyInfo.DifficultyEnum) value);
        }

        protected override void UpdateDropDownOptions()
        {
        }

        private void UpdateDisplayedValues()
        {
            _dropdownField.value = ((int) PlaylistMaker.Instance.Difficulty);

            _dropdownField.RefreshShownValue();
        }
    }
}