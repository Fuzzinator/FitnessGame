using UI.Scrollers;
using UnityEngine;
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
        private void OnEnable()
        {
            _controller.Refresh();
            ColorsManager.Instance.availableColorSetsUpdated.AddListener(_controller.Refresh);
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
    }
}