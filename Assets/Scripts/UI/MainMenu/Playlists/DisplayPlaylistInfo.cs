using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        private Button _playButton;
        
        [SerializeField]
        private Button _editButton;
        
        [SerializeField]
        private Button _deleteButton;

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

        private void Start()
        {
            _playButton.onClick.AddListener(ActiveSceneManager.Instance.LoadBaseLevel);
        }

        public void ShowInfo()
        {
            _playlistTitleCard.SetActive(true);
            _playlistTitle.SetText(PlaylistManager.Instance.CurrentPlaylist.PlaylistName);
            _playlistLength.SetText(PlaylistManager.Instance.CurrentPlaylist.ReadableLength);
            _editButton.gameObject.SetActive(PlaylistManager.Instance.CurrentPlaylist.IsCustomPlaylist);
            _deleteButton.gameObject.SetActive(PlaylistManager.Instance.CurrentPlaylist.IsCustomPlaylist);
            _scrollerController.ReloadScroller();
        }
    }
}