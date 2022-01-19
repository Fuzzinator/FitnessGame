using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using InfoSaving;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Scrollers.Playlists
{
    public class DisplayPlaylistInfo : MonoBehaviour
    {
        public static DisplayPlaylistInfo Instance { get; private set; }

        [SerializeField] private GameObject _playlistTitleCard;

        [SerializeField] private TextMeshProUGUI _playlistTitle;

        [SerializeField] private TextMeshProUGUI _playlistLength;
        [SerializeField] private TextMeshProUGUI _playlistRecordScore;
        [SerializeField] private TextMeshProUGUI _playlistRecordStreak;

        [SerializeField] private Button _playButton;

        [SerializeField] private Button _editButton;

        [SerializeField] private Button _deleteButton;

        [SerializeField] private PlaylistSongScrollerController _scrollerController;

        private CancellationToken _cancellationToken;

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
            _playButton.onClick.AddListener(TryLoadBaseLevel);
            _cancellationToken = this.GetCancellationTokenOnDestroy();
        }

        public async UniTaskVoid ShowInfo()
        {
            _playlistTitleCard.SetActive(true);
            _playlistTitle.SetText(PlaylistManager.Instance.CurrentPlaylist.PlaylistName);
            _playlistLength.SetText(PlaylistManager.Instance.CurrentPlaylist.ReadableLength);
            _editButton.gameObject.SetActive(PlaylistManager.Instance.CurrentPlaylist.IsCustomPlaylist);
            _deleteButton.gameObject.SetActive(PlaylistManager.Instance.CurrentPlaylist.IsCustomPlaylist);
            _scrollerController.ReloadScroller();

            var playlistRecord = await GetPlaylistRecord();
            _playlistRecordScore.SetText(playlistRecord.Score.ToString());
            _playlistRecordStreak.SetText(playlistRecord.Streak.ToString());
        }

        private void TryLoadBaseLevel()
        {
            var playlist = PlaylistManager.Instance.CurrentPlaylist;
            if (!playlist.isValid)
            {
                NotificationManager.RequestNotification(
                    new Notification.NotificationVisuals(
                        $"A song in {playlist.PlaylistName} is missing from this device. Cannot play {playlist.PlaylistName}. Please remove the missing song from the playlist or add it to this device.",
                        "Playlist Invalid",
                        autoTimeOutTime: 1.5f,
                        popUp: true));
            }
            else
            {
                ActiveSceneManager.Instance.LoadBaseLevel();
            }
        }

        private async UniTask<SongAndPlaylistRecord> GetPlaylistRecord()
        {
            var playlist = PlaylistManager.Instance.CurrentPlaylist;
            var playlistFullName = $"{playlist.PlaylistName}-{playlist.Length}-{playlist.Items.Length}";
            if (!PlayerStatsFileManager.PlaylistKeyExists(playlistFullName))
            {
                return new SongAndPlaylistRecord(0, 0);
            }
            return (SongAndPlaylistRecord) await PlayerStatsFileManager.GetPlaylistValue<SongAndPlaylistRecord>(
                playlistFullName, _cancellationToken);
        }
    }
}