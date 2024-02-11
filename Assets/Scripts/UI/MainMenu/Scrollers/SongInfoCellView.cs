using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Scrollers.Playlists
{
    public class SongInfoCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private Image _songArt;

        [SerializeField]
        private TextMeshProUGUI _songDetails;

        [SerializeField]
        private Button _button;

        [SerializeField]
        private Image _previewImage;

        [SerializeField]
        private Sprite _listenSprite;
        [SerializeField]
        private Sprite _muteSprite;

        private SongInfo _songInfo;
        private AvailableSongInfoScrollerController _controller;
        private CancellationToken _cancellationToken;

        private const string SONGINFOFORMAT =
            "<align=left>{0}</style>\n<size=50%>{1}<line-indent=15%>{2}<line-indent=15%>{3}</size></align>";

        private void OnValidate()
        {
            if (_button == null)
            {
                TryGetComponent(out _button);
            }
        }

        public void SetData(SongInfo info, AvailableSongInfoScrollerController controller)
        {
            _songInfo = info;
            _controller = controller;
            var readableLength = info.ReadableLength;
            using (var sb = ZString.CreateStringBuilder(true))
            {
                sb.AppendFormat(SONGINFOFORMAT,
                    info.SongName,
                    info.SongAuthorName,
                    info.LevelAuthorName,
                    readableLength);

                _songDetails.SetText(sb);
            }

            _cancellationToken = controller.CancellationToken;
            GetAndSetImage().Forget();
            //UniTask.RunOnThreadPool(() => , cancellationToken: _cancellationToken);
        }

        public void QuickAddToPlaylist()
        {
            _controller.QuickAddToPlaylist(_songInfo);
        }

        public void SetActiveSongInfo()
        {
            _controller.SetActiveInfo(_songInfo);
        }

        public async UniTaskVoid GetAndSetImage()
        {
            var sprite = await _songInfo.LoadImage(_cancellationToken);
            //await UniTask.SwitchToMainThread(_cancellationToken);
            _songArt.sprite = sprite;
        }

        public void ToggleSongPreview()
        {
            _controller.ToggleSongPreview(_songInfo);
        }
    }
}