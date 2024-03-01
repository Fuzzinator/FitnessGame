using System;
using System.Collections;
using System.Collections.Generic;
using GameModeManagement;
using TMPro;
using UnityEngine;

namespace UI
{
    public class NewPlaylistGameModeSetter : DropdownSetter
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

            PlaylistMaker.Instance.SetGameMode((GameMode) value);
        }

        protected override void UpdateDropDownOptions()
        {
            var targetLength = GameModeExtensions.DifficultyDisplayNames.Length - 2;
            _dropdownField.options =
                new List<TMP_Dropdown.OptionData>(targetLength);
            for (var i = 0; i < targetLength; i++)
            {
                _dropdownField.options.Add(new TMP_Dropdown.OptionData(GameModeExtensions.DifficultyDisplayNames[i]));
                _dropdownField.RefreshShownValue();
            }
        }

        private void UpdateDisplayedValues()
        {
            _dropdownField.value = ((int) PlaylistMaker.Instance.TargetGameMode);

            _dropdownField.RefreshShownValue();
        }
    }
}