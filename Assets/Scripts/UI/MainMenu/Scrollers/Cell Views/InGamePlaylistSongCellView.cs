using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using GameModeManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Scrollers.Playlists
{
    public class InGamePlaylistSongCellView : EnhancedScrollerCellView, HighlightableCellView
    {
        [SerializeField]
        private TextMeshProUGUI _songDetails;
        [SerializeField]
        private Image _highlight;
        [SerializeField]
        private Image _songPercentageDisplay;
        [SerializeField]
        private RectTransform _highlightTransform;

        private bool _active;

        public Image HighlightImage => _highlight;

        public CancellationToken CancellationToken { get; set; }


        private const string INVALIDINDICATOR = "<size=400%><sprite index= 0></size>";
        private const string SONGINFOFORMAT =
            "<style=\"Title\">{0}</style>\n<size=100%><align=center>{1}<line-indent=15%>{2}</align></size>";
        public void SetData(PlaylistItem playlistItem)
        {
            if (_songDetails == null)
            {
                return;
            }

            using (var sb = ZString.CreateStringBuilder(true))
            {
                var targetDifficulty = GetTargetDifficulty(playlistItem);
                var targetGameMode = GetTargetGameMode(playlistItem);
                sb.AppendFormat(SONGINFOFORMAT, playlistItem.SongName, targetDifficulty.Readable(),
                    targetGameMode.Readable());

                _songDetails.SetText(sb);
            }
        }

        public void SetHighlight(bool on)
        {
            _highlight.gameObject.SetActive(on);
            _active = on;
            if (on)
            {
                _songPercentageDisplay.fillAmount = 0;
                DisplaySongPercentage().Forget();
            }
            else
            {
                _songPercentageDisplay.fillAmount = 0;
            }
        }

        private async UniTaskVoid DisplaySongPercentage()
        {
            while(!CancellationToken.IsCancellationRequested && _active)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(.03f), cancellationToken: CancellationToken);
                if(CancellationToken.IsCancellationRequested || !_active)
                {
                    return;
                }
                if(MusicManager.Instance.IsPaused || !MusicManager.Instance.IsPlaying)
                {
                    continue;
                }
                var percent = MusicManager.Instance.GetSongPercentage();
                _songPercentageDisplay.fillAmount = percent;
                _highlightTransform.anchorMin = new Vector2(percent, 0f);
                _highlightTransform.anchorMax = new Vector2(percent, 1f);
            }
        }

        private DifficultyInfo.DifficultyEnum GetTargetDifficulty(PlaylistItem playlistItem)
        {
            if (!PlaylistManager.Instance.OverrideDifficulties &&
                playlistItem.DifficultyEnum != DifficultyInfo.DifficultyEnum.Unset)
            {
                return playlistItem.DifficultyEnum;
            }

            var currentPlaylist = PlaylistManager.Instance.CurrentPlaylist;
            if (currentPlaylist != null && currentPlaylist.DifficultyEnum != DifficultyInfo.DifficultyEnum.Unset)
            {
                return currentPlaylist.DifficultyEnum;
            }

            return DifficultyInfo.DifficultyEnum.Normal;
        }

        private GameMode GetTargetGameMode(PlaylistItem playlistItem)
        {
            if (!PlaylistManager.Instance.OverrideGameModes && playlistItem.TargetGameMode != GameMode.Unset)
            {
                return playlistItem.TargetGameMode;
            }

            var currentPlaylist = PlaylistManager.Instance.CurrentPlaylist;
            if (currentPlaylist != null && currentPlaylist.TargetGameMode != GameMode.Unset)
            {
                return currentPlaylist.TargetGameMode;
            }

            return GameMode.Normal;
        }
    }
}