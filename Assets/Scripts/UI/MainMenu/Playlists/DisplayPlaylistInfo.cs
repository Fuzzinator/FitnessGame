using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using InfoSaving;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace UI.Scrollers.Playlists
{
    public class DisplayPlaylistInfo : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup _playlistTitleCard;

        [SerializeField]
        TextMeshProUGUI _playlistTitle;

        [SerializeField]
        TextMeshProUGUI _playlistName;
        [SerializeField]
        private TextMeshProUGUI _playlistLength;
        [SerializeField]
        private TextMeshProUGUI _playlistRecordScore;
        [SerializeField]
        TextMeshProUGUI _playlistRecordStreak;

        [SerializeField] private Button _playButton;

        [SerializeField] private Button _editButton;

        [SerializeField] private Button _deleteButton;

        [SerializeField] private PlaylistSongScrollerController _scrollerController;

        private CancellationToken _cancellationToken;

        private void OnEnable()
        {
            PlaylistManager.Instance.currentPlaylistUpdated.AddListener(RequestShowInfo);
            SongInfoFilesReader.Instance.SongAdded.AddListener(_scrollerController.CheckAddedSong);
            ShowInfo().Forget();
        }

        private void OnDisable()
        {
            PlaylistManager.Instance.currentPlaylistUpdated.RemoveListener(RequestShowInfo);
        }

        private void Start()
        {
            //_playButton.onClick.AddListener(TryLoadBaseLevel);
            _cancellationToken = this.GetCancellationTokenOnDestroy();
            if (PlaylistManager.Instance.CurrentPlaylist != null)
            {
                _playlistTitleCard.interactable = PlaylistManager.Instance.CurrentPlaylist.isValid;
            }
            else
            {
                _playlistTitleCard.interactable = false;
            }
        }

        public void RequestShowInfo(Playlist playlist)
        {
            ShowInfo().Forget();
        }

        public async UniTaskVoid ShowInfo()
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
            var currentPlaylist = PlaylistManager.Instance.CurrentPlaylist;
            if (currentPlaylist == null)
            {
                _playlistTitleCard.interactable = false;
                return;
            }

            _playlistTitleCard.interactable = true;
            //_playlistTitleCard.interactable = currentPlaylist.isValid;
            _playlistTitle.text = currentPlaylist.PlaylistName;
            _playlistName.text = currentPlaylist.PlaylistName;
            _playlistLength.text = currentPlaylist.ReadableLength;
            _playButton.interactable = currentPlaylist.isValid;
            _editButton.gameObject.SetActive(currentPlaylist.IsCustomPlaylist);
            _deleteButton.gameObject.SetActive(currentPlaylist.IsCustomPlaylist);
            _scrollerController.ReloadScroller();

            var playlistRecords = await GetPlaylistRecords();
            ulong score = 0;
            var streak = 0;
            if (playlistRecords != null && playlistRecords.Length > 0)
            {
                score = playlistRecords[0].Score;
                streak = playlistRecords[0].Streak;
            }
            using (var sb = ZString.CreateStringBuilder(true))
            {
                sb.Append(score);

                var buffer = sb.AsArraySegment();
                _playlistRecordScore.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
            }
            using (var sb = ZString.CreateStringBuilder(true))
            {
                sb.Append(streak);

                var buffer = sb.AsArraySegment();
                _playlistRecordStreak.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
            }
        }

        private async UniTask<PlaylistRecord[]> GetPlaylistRecords()
        {
            var playlist = PlaylistManager.Instance.CurrentPlaylist;
            if (playlist == null)
            {
                Debug.LogError("Trying to get record but playlist is null.");
                return null;
            }
            return await PlayerStatsFileManager.TryGetPlaylistRecords(playlist, _cancellationToken);
        }
    }
}