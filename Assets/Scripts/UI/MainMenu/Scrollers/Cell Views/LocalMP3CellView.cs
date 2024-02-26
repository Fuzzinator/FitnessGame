using Cysharp.Text;
using EnhancedUI.EnhancedScroller;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace UI.Scrollers
{
    public class LocalMP3CellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private Image _songArt;

        [SerializeField]
        private TextMeshProUGUI _songDetails;

        [SerializeField]
        private Button _button;

        private int _index;
        private AvailableLocalMp3sScrollerController _controller;

        private const string SONGINFOFORMAT =
            "<align=left>{0}</style>\n<size=50%>{1}<line-indent=15%>{2}<line-indent=15%>{3}</size></align>";

        public void SetData(TagLib.File tagFile, int index, AvailableLocalMp3sScrollerController controller)
        {
            _index = index;
            _controller = controller;
            SetText(tagFile);
            TrySetImage(tagFile);
        }

        private void SetText(TagLib.File tagFile)
        {
            var performersName = "Unknown";
            if (tagFile.Tag.Performers != null && tagFile.Tag.Performers.Length > 0)
            {
                performersName = string.Join(" & ", tagFile.Tag.Performers);
            }
            else if (tagFile.Tag.AlbumArtists != null && tagFile.Tag.AlbumArtists.Length > 0)
            {
                performersName = string.Join(" & ", tagFile.Tag.AlbumArtists);
            }
            TimeSpan duration = tagFile.Properties.Duration;
            string songLength = string.Format("{0}:{1:00}", (int)duration.TotalMinutes, duration.Seconds);
            using (var sb = ZString.CreateStringBuilder(true))
            {
                sb.AppendFormat(SONGINFOFORMAT,
                    tagFile.Tag.Title ?? "Unknown Artist",
                    performersName,
                    songLength,
                    tagFile.Tag.Album ?? "Unknown Album");

                _songDetails.SetText(sb);
            }
        }

        private void TrySetImage(TagLib.File tagFile)
        {
            _songArt.sprite = null;
            _songArt.enabled = false;
            byte[] bytes = null;
            if (tagFile.Tag.Pictures.Length > 0)
            {
                if (tagFile.Tag.Pictures[0].Data.Data != null)
                {
                    bytes = tagFile.Tag.Pictures[0].Data.Data;
                }
            }
            if (bytes == null)
            {
                return;
            }
            var image = new Texture2D(2, 2);
            image.LoadImage(bytes);

            if (image == null)
            {
                return;
            }

            _songArt.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height),
                Vector2.one * .5f, 100, 0, SpriteMeshType.FullRect);

            _songArt.enabled = true;
        }

        public void TryConvertSong()
        {
            _controller.TryConvertSong(_index);
        }

        public void ToggleSongPreview()
        {
            _controller.ToggleSongPreview(_index);
        }
    }
}