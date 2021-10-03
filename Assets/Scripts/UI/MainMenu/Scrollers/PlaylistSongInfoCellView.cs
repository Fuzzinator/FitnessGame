using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;

namespace UI.Scrollers.Playlists
{
    public class PlaylistSongInfoCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private TextMeshProUGUI _songName;

        [SerializeField]
        private TextMeshProUGUI _songDifficulty;
        public void SetData(PlaylistItem item)
        {
            _songName.SetText(item.SongName);
            _songDifficulty.SetText(item.Difficulty);
        }
    }
}