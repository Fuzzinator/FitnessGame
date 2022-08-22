using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;

namespace UI.Scrollers.Playlists
{
    public class PlaylistCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private TextMeshProUGUI _playlistName;

        private Playlist _playlist;

        private const string INVALID = "<sprite index=0>";
        
        public void SetData(Playlist playlist)
        {
            _playlist = playlist;
            using (var sb = ZString.CreateStringBuilder(true))
            {
                if (!_playlist.isValid)
                {
                    sb.Append(INVALID);
                }
                sb.Append(_playlist.PlaylistName);
                
                _playlistName.SetText(sb);
            }
        }

        public void SetActivePlaylist()
        {
            PlaylistManager.Instance.SetActivePlaylist(_playlist);
            DisplayPlaylistInfo.Instance.ShowInfo().Forget();
        }
    }
}