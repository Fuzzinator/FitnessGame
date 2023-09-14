using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Scrollers
{
    public class CustomSkyboxCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private TextMeshProUGUI _skyboxNameField1;
        [SerializeField]
        private Image _skyboxThumbnail1;
        [SerializeField]
        private MultiColorButton _skyboxButton1;

        [SerializeField]
        private TextMeshProUGUI _skyboxNameField2;
        [SerializeField]
        private Image _skyboxThumbnail2;
        [SerializeField]
        private MultiColorButton _skyboxButton2;

        [SerializeField]
        private RectTransform _editSkyboxContainer;
        [SerializeField]
        private EditUGUITextField _textEditor;

        private int _index;
        private int _editingIndex;
        private CustomSkyboxesScrollerController _controller;
        private CancellationToken _cancellationToken;

        private readonly Vector2 _editingTopOffset = new Vector2(0, -5);
        private readonly Vector2 _editingBottomOffset = new Vector2(0, -115);

        public void SetData(int index, CustomSkyboxesScrollerController controller)
        {
            if (_cancellationToken == null)
            {
                _cancellationToken = this.GetCancellationTokenOnDestroy();
            }

            _index = index;
            _controller = controller;

            if (index < CustomEnvironmentsController.CustomSkyboxesCount)
            {
                var skyboxName = CustomEnvironmentsController.GetSkyboxName(index);
                _skyboxNameField1.SetTextZeroAlloc(skyboxName, true);
                var skyboxPath = CustomEnvironmentsController.GetSkyboxPath(index);
                SetSprite(_skyboxThumbnail1, skyboxName, skyboxPath, index).Forget();
                _skyboxButton1.gameObject.SetActive(true);
            }
            else
            {
                _skyboxButton1.gameObject.SetActive(false);
                _skyboxNameField1.ClearText();
                _skyboxThumbnail1.sprite = null;
            }
            if (index + 1 < (CustomEnvironmentsController.CustomSkyboxesCount))
            {
                var skyboxName = CustomEnvironmentsController.GetSkyboxName(index + 1);
                _skyboxNameField2.SetTextZeroAlloc(skyboxName, true);
                var skyboxPath = CustomEnvironmentsController.GetSkyboxPath(index + 1);
                SetSprite(_skyboxThumbnail2, skyboxName, skyboxPath, index).Forget();
                _skyboxButton2.gameObject.SetActive(true);
            }
            else
            {
                _skyboxButton2.gameObject.SetActive(false);
                _skyboxNameField2.ClearText();
                _skyboxThumbnail2.sprite = null;
            }
        }

        private async UniTaskVoid SetSprite(Image image, string skyboxName, string skyboxPath, int index)
        {
            image.sprite = null;
            image.color = Color.gray;
            if (string.IsNullOrWhiteSpace(skyboxName))
            {
                return;
            }
            await UniTask.DelayFrame(1);
            var sprite = await CustomEnvironmentsController.GetEnvironmentThumbnailAsync(skyboxName, skyboxPath, _cancellationToken);
            if (index == _index)
            {
                image.sprite = sprite;
            }
        }

        public void SelectSkybox(int index)
        {
            var targetImage = index == 0 ? _skyboxThumbnail1 : _skyboxThumbnail2;
            _controller.SelectSkybox(_index + index, targetImage.sprite);
        }

        public void EditSkybox(int index)
        {
            _editingIndex = index;
            _editSkyboxContainer.anchoredPosition = index == 0 ? _editingTopOffset : _editingBottomOffset;
            _editSkyboxContainer.gameObject.SetActive(true);
        }

        public void RenameSkybox()
        {
            var skyboxName = GetSkyboxName();
            var suffixIndex = skyboxName.LastIndexOf(".");
            var suffix = skyboxName.Substring(suffixIndex);
            skyboxName = skyboxName.Substring(0, suffixIndex);

            var targetText = _editingIndex == 0 ? _skyboxNameField1 : _skyboxNameField2;
            _textEditor.SetTargetText(targetText);
            _textEditor.StartEditTextField(skyboxName, suffix);
            _editSkyboxContainer.gameObject.SetActive(false);
        }
        public void CompleteRenameSkybox(string newName)
        {
            var skyboxName = GetSkyboxName();
            var targetText = _editingIndex == 0 ? _skyboxNameField1 : _skyboxNameField2;
            if (string.IsNullOrWhiteSpace(newName))
            {
                targetText.SetTextZeroAlloc(skyboxName, true);
            }
            else
            {
                var canRename = CustomEnvironmentsController.RenameSkybox(skyboxName, newName);
                if (canRename)
                {
                    targetText.SetTextZeroAlloc(newName, true);
                    _controller.RenameComplete(newName);
                }
                else
                {
                    targetText.SetTextZeroAlloc(skyboxName, true);
                    var visuals = _controller.RenameFailedVisuals;
                    NotificationManager.RequestNotification(visuals);
                }
            }
        }

        public void DeleteSkybox()
        {
            var visuals = CustomEnvironmentsController.ConfirmDeleteSkybox;
            NotificationManager.RequestNotification(visuals, () => ConfirmDeleteSkybox());
        }

        public void ConfirmDeleteSkybox()
        {
            var skyboxName = GetSkyboxName();
            CustomEnvironmentsController.DeleteSkybox(skyboxName);
            _editSkyboxContainer.gameObject.SetActive(false);
            //_controller.DeleteSkybox(skyboxName);
            _controller.Refresh();
        }

        private string GetSkyboxName()
        {
            return CustomEnvironmentsController.GetSkyboxName(_index + _editingIndex);
        }
    }
}