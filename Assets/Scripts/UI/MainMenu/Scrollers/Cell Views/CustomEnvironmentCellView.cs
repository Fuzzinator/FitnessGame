using UnityEngine.UI;
using Cysharp.Text;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UI.Scrollers.Playlists
{
    public class CustomEnvironmentCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private TextMeshProUGUI _environmentName;
        [SerializeField]
        private Image _environmentThumbnail;

        private CustomEnvironment _environment;
        private int _index;
        private AvailableCustomEnvironmentsScrollerController _controller;
        private CancellationToken _cancellationToken;

        private const string INVALID = "<sprite index=0>";
        
        public void SetData(CustomEnvironment environment, int index, AvailableCustomEnvironmentsScrollerController controller)
        {
            if(_cancellationToken == null)
            {
                _cancellationToken = this.GetCancellationTokenOnDestroy();
            }

            _environment = environment;
            _index = index;
            _controller = controller;
            if (_environment == null)
            {
                return;
            }

            using (var sb = ZString.CreateStringBuilder(true))
            {
                if (!_environment.isValid)
                {
                    sb.Append(INVALID);
                }
                sb.Append(_environment.EnvironmentName);
                
                _environmentName.SetText(sb);
            }
            SetSprite(_environmentThumbnail, _environment.SkyboxPath, index).Forget();
        }

        private async UniTaskVoid SetSprite(Image image, string skyboxName, int index)
        {
            if (string.IsNullOrWhiteSpace(skyboxName))
            {
                image.sprite = null;
                image.color = Color.gray;
                return;
            }
            await UniTask.DelayFrame(1);
            var sprite = await CustomEnvironmentsController.GetEnvironmentThumbnailAsync(skyboxName, _cancellationToken);
            if (index == _index)
            {
                image.sprite = sprite;
                image.color = Color.white;
            }
        }

        public void SetSelected()
        {
            _controller.SetActiveEnvironment(_index);
        }
    }
}