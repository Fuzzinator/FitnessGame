using UI.Scrollers;
using UnityEngine;
using UnityEngine.UI;
using ColorSet = ColorsManager.ColorSet;

namespace UI
{
    public class ColorSetSelector : MonoBehaviour
    {
        [SerializeField, CustomAttributes.Expandable]
        private AvailableColorSetsScrollerController _controller;

        [SerializeField]
        private CanvasGroup _canvasGroup;

        [SerializeField]
        private ColorSetEditor _colorSetEditor;

        [SerializeField]
        private MultiGraphicButton _deleteButton;
        [SerializeField]
        private MultiGraphicButton _resetButton;

        private void OnEnable()
        {
            _controller.Refresh();
            ColorsManager.Instance.availableColorSetsUpdated.AddListener(_controller.Refresh);
            ColorsManager.Instance.activeColorSetUpdated.AddListener(CheckActiveColorSet);
            CheckActiveColorSet(ColorsManager.Instance.ActiveColorSet);
        }

        private void OnDisable()
        {
            RequestCloseSetEditor();
            ColorsManager.Instance.availableColorSetsUpdated.RemoveListener(_controller.Refresh);
        }

        public void SetActiveColorSet(ColorSet set, int index)
        {
            ColorsManager.Instance.SetActiveColorSet(set, index);
        }

        public void RequestOpenSetEditor(ColorSet set, int index)
        {
            _colorSetEditor.RequestShowEditor(set, index);
        }

        public void RequestCloseSetEditor()
        {
            _colorSetEditor.CloseEditor();
        }

        private void CheckActiveColorSet(ColorSet set)
        {
            var notDefault = ColorsManager.Instance.ActiveSetIndex != 0;
            _deleteButton.interactable = notDefault;
            _resetButton.interactable = notDefault;
        }
    }
}