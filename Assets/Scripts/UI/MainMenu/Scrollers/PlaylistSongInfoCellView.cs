using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using GameModeManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Scrollers.Playlists
{
    public class PlaylistSongInfoCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private TextMeshProUGUI _songDetails;

        private const string INVALIDINDICATOR = "<sprite index=1>";

        private const string SONGINFOFORMAT =
            "<align=center>{0}</style>\n<size=50%>{1}<line-indent=15%>{2}</size></align>";

        private PlaylistItem _playlistItem;
        
        public void SetData(PlaylistItem item)
        {
            _playlistItem = item;
            SetDataAsync(item).Forget();
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
        public void RemovePlaylistItem()
        {
            if (PlaylistMaker.Instance != null)
            {
                PlaylistMaker.Instance.RemovePlaylistItem(_playlistItem);
            }
        }
    }
}