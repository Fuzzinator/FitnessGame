using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Scrollers.Playlists
{
    public class PlaylistCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private TextMeshProUGUI _playlistName;

        [SerializeField] 
        private Image _invalidIndicator;
        private Playlist _playlist;

        public void SetData(Playlist playlist)
        {
            _playlist = playlist;
            _playlistName.SetText(playlist.PlaylistName);
            _invalidIndicator.enabled = !_playlist.isValid;
        }

        public void SetActivePlaylist()
        {
            PlaylistManager.Instance.SetActivePlaylist(_playlist);
            DisplayPlaylistInfo.Instance.ShowInfo();
        }
    }
}