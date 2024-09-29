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
        private TextMeshProUGUI _playlistRecordStreak;
        [SerializeField]
        private TextMeshProUGUI _editButtonText;
        [SerializeField]
        private TextMeshProUGUI _deleteButtonText;
        [SerializeField]
        private TextMeshProUGUI _hideButtonText;

        [SerializeField] 
        private Button _playButton;

        [SerializeField]
        private Button _editButton;

        [SerializeField]
        private Button _deleteButton;

        [SerializeField]
        private Button _hideButton;

        [SerializeField]
        private PlaylistSongScrollerController _scrollerController;

        private CancellationToken _cancellationToken;

        private float _songSpeedModifier = 1f;

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

            _playlistTitle.text = currentPlaylist.PlaylistName;
            _playlistName.text = currentPlaylist.PlaylistName;
            _playlistLength.text = currentPlaylist.GetReadableLength(_songSpeedModifier);
            _playButton.interactable = currentPlaylist.isValid;

            var isCustomPlaylist = currentPlaylist.IsCustomPlaylist;
            _editButton.gameObject.SetActive(isCustomPlaylist);
            _editButtonText.gameObject.SetActive(isCustomPlaylist);

            _deleteButton.gameObject.SetActive(isCustomPlaylist);
            _deleteButtonText.gameObject.SetActive(isCustomPlaylist);

            _hideButton.gameObject.SetActive(!isCustomPlaylist);
            _hideButtonText.gameObject.SetActive(!isCustomPlaylist);

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
        public void SongSpeedModValueChanged(float value)
        {
            _songSpeedModifier = SongSliderToPlaylistSpeedMod(value);

            ShowInfo().Forget();
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

        private float SongSliderToPlaylistSpeedMod(float sliderValue)
        {
            switch ((int)sliderValue)
            {
                case 0:
                    return .75f;
                case 1:
                    return .875f;
                case 2:
                    return 1;
                case 3:
                    return 1.125f;
                case 4:
                    return 1.25f;
                default:
                    return 1;
            }
        }
    }
}