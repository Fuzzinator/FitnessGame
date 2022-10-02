using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class DifficultyDisplaySetter : DropdownSetter
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

            PlaylistManager.Instance.SetDifficulty((DifficultyInfo.DifficultyEnum) value);
        }



        protected override void UpdateDropDownOptions()
        {
        }

        private void UpdateDisplayedValues(Playlist playlist)
        {
            var difficulty = playlist.DifficultyEnum;
            _dropdownField.value = ((int) difficulty);

            _dropdownField.RefreshShownValue();
        }
    }
}