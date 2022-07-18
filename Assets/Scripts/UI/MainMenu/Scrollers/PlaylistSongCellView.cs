using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
    public class PlaylistSongCellView : EnhancedScrollerCellView, HighlightableCellView
    {
        [SerializeField]
        [FormerlySerializedAs("_songName")] 
        private TextMeshProUGUI _songDetails;

        [SerializeField]
        private Image _highlight;

        public Image HighlightImage => _highlight;
        
        private const string INVALID = "<sprite index=1>";
        private const string SONGINFOFORMAT =
            "<align=center>{0}</style>\n<size=50%>{1}<line-indent=15%>{2}</size></align>";

        public void SetData(PlaylistItem playlist)
        {
            UpdateDisplay(playlist).Forget();
        }
        
        private async UniTaskVoid UpdateDisplay(PlaylistItem playlistItem)
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
                    sb.Append(INVALID);
                }
                sb.AppendFormat(SONGINFOFORMAT, playlistItem.SongName, playlistItem.Difficulty, playlistItem.TargetGameMode.GetDisplayName());

                _songDetails.SetText(sb);
            }
        }

        public void SetHighlight(bool on)
        {
            _highlight.enabled = on;
        }
    }
}

public interface HighlightableCellView
{
    public Image HighlightImage { get; }
    public void SetData(PlaylistItem playlistItem);
    public void SetHighlight(bool on);
}