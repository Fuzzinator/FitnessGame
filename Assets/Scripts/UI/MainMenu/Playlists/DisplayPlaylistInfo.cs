using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI.Scrollers.Playlists
{
    public class DisplayPlaylistInfo : MonoBehaviour
    {
        public static DisplayPlaylistInfo Instance { get; private set; }

        [SerializeField]
        private GameObject _playlistTitleCard;

        [SerializeField]
        private TextMeshProUGUI _playlistTitle;

        [SerializeField]
        private TextMeshProUGUI _playlistLength;

        [SerializeField]
        private PlaylistSongScrollerController _scrollerController;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public void ShowInfo()
        {
            _playlistTitleCard.SetActive(true);
            _playlistTitle.SetText(PlaylistManager.Instance.CurrentPlaylist.PlaylistName);
            _playlistLength.SetText(PlaylistManager.Instance.CurrentPlaylist.ReadableLength);
            _scrollerController.ReloadScroller();
        }
    }
}