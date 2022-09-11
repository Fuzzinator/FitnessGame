using System.Collections;
using System.Collections.Generic;
using BeatSaverSharp.Models;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Scrollers.BeatsaverIntegraton
{
    public class BeatSaverSongCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private Image songImage;
        [SerializeField]
        private TextMeshProUGUI _songDetails;

        private Beatmap _beatmap;
        private BeatSaverSongsScrollerController _controller;
        
        private const string SONGINFOFORMAT =
            "<align=left>{0}</style>\n<size=50%><b>Song Author:</b> {1}<line-indent=10%><b>Level Author:</b> {2}<line-indent=10%><b>Song Score:</b> {3}</size></align>";
        
        public void SetData(Beatmap item, BeatSaverSongsScrollerController controller)
        {
            using (var sb = ZString.CreateStringBuilder(true))
            {
                sb.AppendFormat(SONGINFOFORMAT, 
                    item.Metadata.SongName, 
                    item.Metadata.SongAuthorName, 
                    item.Metadata.LevelAuthorName,
                    item.Stats.Score*100);

                _songDetails.SetText(sb);
            }
            
            _beatmap = item;
            _controller = controller;
            
            GetAndSetImage(item).Forget();
        }

        public void Selected()
        {
            _controller.SetActiveBeatmap(_beatmap);
        }

        private async UniTaskVoid GetAndSetImage(Beatmap item)
        {
            var imageBytes = await item.LatestVersion.DownloadCoverImage(token: _controller.CancellationToken);
            if (imageBytes != null)
            {
                await UniTask.SwitchToMainThread(_controller.CancellationToken);
                var image = new Texture2D(1, 1);
                image.LoadImage(imageBytes);
                songImage.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * .5f, 100f);
            }
        }
    }
}