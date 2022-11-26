using System;
using System.Collections;
using System.Collections.Generic;
using GameModeManagement;
using TMPro;
using UnityEngine;

namespace UI
{
    public class GameTypeDisplaySetter : DropdownSetter
    {
        private bool PlaylistAvailable => PlaylistManager.Instance != null;

        private void OnEnable()
        {
            if (!PlaylistAvailable)
            {
                return;
            }

            UpdateDisplayedValues(PlaylistManager.Instance.CurrentPlaylist);
            PlaylistManager.Instance.currentPlaylistUpdated.AddListener(UpdateDisplayedValues);
        }

        private void OnDisable()
        {
            PlaylistManager.Instance.currentPlaylistUpdated.RemoveListener(UpdateDisplayedValues);
        }

        public override void SetDropdownOption(int value)
        {
            if (!PlaylistAvailable)
            {
                return;
            }

            PlaylistManager.Instance.SetGameMode((GameMode) value);
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

        private void UpdateDisplayedValues(Playlist playlist)
        {
            if (playlist == null)
            {
                return;
            }
            
            var gameMode = playlist.TargetGameMode;
            _dropdownField.value = (int) gameMode;

            _dropdownField.RefreshShownValue();
        }
    }
}