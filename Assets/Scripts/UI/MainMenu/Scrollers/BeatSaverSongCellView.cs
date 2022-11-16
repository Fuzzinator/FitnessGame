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
        [FormerlySerializedAs("songImage")] [SerializeField]
        private Image _songImage;
        [SerializeField]
        private Image _downloadedMarker;
        [SerializeField]
        private TextMeshProUGUI _songDetails;

        private Beatmap _beatmap;
        private BeatSaverSongsScrollerController _controller;
        
        private const string SONGINFOFORMAT =
            "<align=left>{0}</style>\n<size=50%><b>Song Author:</b> {1}<line-indent=10%><b>Level Author:</b> {2}<line-indent=10%><b>Song Score:</b> {3}</size></align>";
        
        public void SetData(Beatmap item, BeatSaverSongsScrollerController controller)
        {
            if (_beatmap == item)
            {
                return;
            }
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
            SetDownloadedMarker();
            GetAndSetImage(item).Forget();
        }

        public void Selected()
        {
            _controller.SetActiveBeatmap(_beatmap);
            _controller.SetSelectedCellView(this);
        }

        private void SetDownloadedMarker()
        {
            var alreadyDownloaded = SongInfoFilesReader.Instance.availableSongs.Exists((song) => song == _beatmap);
            _downloadedMarker.enabled = alreadyDownloaded;
        }

        public void SetDownloaded(bool downloaded)
        {
            _downloadedMarker.enabled = downloaded;
        }
        
        private async UniTaskVoid GetAndSetImage(Beatmap item)
        {
            var imageBytes = await item.LatestVersion.DownloadCoverImage(token: _controller.CancellationToken);
            if (imageBytes != null)
            {
                await UniTask.SwitchToMainThread(_controller.CancellationToken);
                var image = new Texture2D(1, 1);
                image.LoadImage(imageBytes);
                _songImage.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * .5f, 100f);
            }
        }
    }
}