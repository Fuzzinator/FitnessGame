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

        private const string INVALIDINDICATOR = "<size=400%><sprite index= 0></size>";

        private const string SONGINFOFORMAT =
            "<style=\"Title\">{0}</style>\n<size=100%><align=center>{1}<line-indent=15%>{2}</align></size>";
        
        [SerializeField]
        private Image _highlight;
        public Image HighlightImage => _highlight;
        
        public CancellationToken CancellationToken { get; set; }

        public void SetData(PlaylistItem playlistItem)
        {
            SetDataAsync(playlistItem).Forget();
        }

        public void SetHighlight(bool on)
        {
            _highlight.gameObject.SetActive(on);
        }

        private async UniTaskVoid SetDataAsync(PlaylistItem playlistItem)
        {
            var isValid = await PlaylistValidator.IsValid(playlistItem);
            if (_songDetails == null)
            {
                return;
            }
            using (var sb = ZString.CreateStringBuilder(true))
            {
                if (!isValid)
                {
                    sb.Append(INVALIDINDICATOR);
                }
                sb.AppendFormat(SONGINFOFORMAT, playlistItem.SongName, playlistItem.Difficulty, playlistItem.TargetGameMode.GetDisplayName());

                _songDetails.SetText(sb);
            }
        }
    }
}