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
    public class PlaylistSongCellView : EnhancedScrollerCellView, HighlightableCellView
    {
        [SerializeField]
        private Image _songImage;
        [SerializeField]
        [FormerlySerializedAs("_songName")] 
        private TextMeshProUGUI _songDetails;

        [SerializeField]
        private Image _highlight;

        public Image HighlightImage => _highlight;

        public CancellationToken CancellationToken { get; set; }

        private const string INVALID = "<sprite index=1>";
        private const string SONGINFOFORMAT = "<align=left>{0}</style>\n<size=50%>{1}<line-indent=15%>{2}</size></align>";

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
                    _songDetails.margin = new Vector4(35, 0, 0, 0);
                }
                else
                {
                    _songDetails.margin = Vector4.one;
                }
                sb.AppendFormat(SONGINFOFORMAT, playlistItem.SongName, playlistItem.Difficulty, 
                            playlistItem.TargetGameMode.GetDisplayName());

                _songDetails.SetText(sb);
            }

            if (_songImage == null)
            {
                return;
            }
            GetAndSetImage(playlistItem.SongInfo).Forget();
        }

        public void SetHighlight(bool on)
        {
            _highlight.enabled = on;
        }
        
        public async UniTaskVoid GetAndSetImage(SongInfo info)
        {
            var image = await info.LoadImage(CancellationToken);
            if (image == null)
            {
                _songImage.sprite = null;
            }
            var newSprite = Sprite.Create(image, new Rect(0,0, image.width, image.height),
                                     Vector2.one *.5f, 100f);
            _songImage.sprite = newSprite;
        }
    }
}

public interface HighlightableCellView
{
    public Image HighlightImage { get; }
    public void SetData(PlaylistItem playlistItem);
    public void SetHighlight(bool on);

    public CancellationToken CancellationToken { get; set; }
}