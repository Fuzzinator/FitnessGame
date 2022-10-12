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
            _dropdownField.options =
                new List<TMP_Dropdown.OptionData>(GameModeExtensions.DifficultyDisplayNames.Length);
            for (var i = 0; i < GameModeExtensions.DifficultyDisplayNames.Length; i++)
            {
                _dropdownField.options.Add(new TMP_Dropdown.OptionData(GameModeExtensions.DifficultyDisplayNames[i]));
                _dropdownField.RefreshShownValue();
            }
        }

        private void UpdateDisplayedValues()
        {
            _dropdownField.value = ((int) PlaylistMaker.Instance.CurrentGameMode);

            _dropdownField.RefreshShownValue();
        }
    }
}