using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Scrollers.Playlists
{
    public class PlaylistSongInfoCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private TextMeshProUGUI _songName;

        [SerializeField]
        private TextMeshProUGUI _songDifficulty;

        [SerializeField] private Image _invalidIndicator;
        public async void SetData(PlaylistItem item)
        {
            _songName.SetText(item.SongName);
            _songDifficulty.SetText(item.Difficulty);
            var isValid = await PlaylistValidator.IsValid(item);
            _invalidIndicator.enabled = !isValid;
        }
    }
}