using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Scrollers
{
    public class AvaillableImagesCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private TextMeshProUGUI _skyboxNameField1;
        [SerializeField]
        private Image _skyboxThumbnail1;
        [SerializeField]
        private Image _thumbnail1Highlight1;
        [SerializeField]
        private MultiColorButton _skyboxButton1;

        [SerializeField]
        private TextMeshProUGUI _skyboxNameField2;
        [SerializeField]
        private Image _skyboxThumbnail2;
        [SerializeField]
        private Image _thumbnail1Highlight2;
        [SerializeField]
        private MultiColorButton _skyboxButton2;


        private int _index;
        private AvailableImagesScrollerController _controller;
        private CancellationToken _cancellationToken;
        private bool _image1Selected;
        private bool _image2Selected;

        public void SetData(int index, AvailableImagesScrollerController controller)
        {
            if (_cancellationToken == null)
            {
                _cancellationToken = this.GetCancellationTokenOnDestroy();
            }

            _index = index;
            _controller = controller;
            _image1Selected = _controller.IsSelected(_index);
            _image2Selected = _controller.IsSelected(_index+1);

            _thumbnail1Highlight1.enabled = _image1Selected;
            _thumbnail1Highlight2.enabled = _image2Selected;

            if (index < CustomEnvironmentsController.ImagesInDownloadsCount)
            {
                var imageName = CustomEnvironmentsController.GetDownloadsImageName(index);
                _skyboxNameField1.SetTextZeroAlloc(imageName, true);
                var imagePath = CustomEnvironmentsController.GetDownloadsImagePath(index);
                SetSprite(_skyboxThumbnail1, imagePath, index).Forget();
            }
            else
            {
                _skyboxButton1.gameObject.SetActive(false);
                _skyboxNameField1.ClearText();
                _skyboxThumbnail1.sprite = null;
            }
            if (index + 1 < (CustomEnvironmentsController.ImagesInDownloadsCount))
            {
                var imageName = CustomEnvironmentsController.GetDownloadsImageName(index + 1);
                _skyboxNameField2.SetTextZeroAlloc(imageName, true);
                var imagePath = CustomEnvironmentsController.GetDownloadsImagePath(index + 1);
                SetSprite(_skyboxThumbnail2, imagePath, index).Forget();
            }
            else
            {
                _skyboxButton2.gameObject.SetActive(true);
                _skyboxNameField2.ClearText();
                _skyboxThumbnail2.sprite = null;
            }
        }

        private async UniTaskVoid SetSprite(Image image, string skyboxName, int index)
        {
            if (string.IsNullOrWhiteSpace(skyboxName))
            {
                return;
            }
            await UniTask.DelayFrame(1);
            var sprite = await CustomEnvironmentsController.GetEnvironmentThumbnailAsync(skyboxName, _cancellationToken);
            if (index == _index)
            {
                image.sprite = sprite;
            }
        }

        public void SetSelected(int index)
        {
            var targetImage = _index == index ? _thumbnail1Highlight1 : _thumbnail1Highlight2;
            var selected = _controller.SelectImage(_index + index);
            targetImage.enabled = selected;
        }
    }
}