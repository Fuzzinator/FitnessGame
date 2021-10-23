using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Scrollers.Playlists
{
    public class PlaylistSongCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private TextMeshProUGUI _songName;

        [SerializeField]
        private TextMeshProUGUI _songDifficulty;

        [FormerlySerializedAs("_beatsPerMinute")] [SerializeField]
        private TextMeshProUGUI _songLength;

        [SerializeField]
        private Image _highlight;

        private bool _isHighlightNull;

        private void Start()
        {
            _isHighlightNull = _highlight == null;
        }

        public void SetData(PlaylistItem playlist)
        {
            _songName?.SetText(playlist.SongName);
            _songDifficulty?.SetText(playlist.Difficulty);
            _songLength?.SetText(playlist.SongInfo.ReadableLength);
            if (_isHighlightNull)
            {
                SetHighlight(false);
            }
        }

        public void SetHighlight(bool on)
        {
            _highlight.enabled = on;
        }
    }
}