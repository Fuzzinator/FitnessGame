using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;

namespace UI.Scrollers.Playlists
{
    public class PlaylistSongCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private TextMeshProUGUI _songName;

        [SerializeField]
        private TextMeshProUGUI _songDifficulty;

        [SerializeField]
        private TextMeshProUGUI _beatsPerMinute;

        public void SetData(PlaylistItem playlist)
        {
            _songName.SetText(playlist.SongName);
            _songDifficulty.SetText(playlist.Difficulty);
            _beatsPerMinute.SetText(playlist.SongInfo.BeatsPerMinute.ToString(CultureInfo.InvariantCulture));
        }
    }
}