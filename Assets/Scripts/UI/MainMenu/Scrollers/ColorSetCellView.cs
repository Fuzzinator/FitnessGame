using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Scrollers
{
    public class ColorSetCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private Image _highlight;

        [SerializeField]
        private Image _leftColor;
        [SerializeField]
        private Image _rightColor;
        [SerializeField]
        private Image _blockColor;
        [SerializeField]
        private Image _obstacleColor;

        private ColorsManager.ColorSet _colorSet;
        private AvailableColorSetsScrollerController _controller;

        public void SetData(ColorsManager.ColorSet colorSet, AvailableColorSetsScrollerController controller)
        {
            _colorSet = colorSet;
            _leftColor.color = colorSet.LeftController;
            _rightColor.color = colorSet.RightController;
            _blockColor.color = colorSet.BlockColor;
            _obstacleColor.color = colorSet.ObstacleColor;
            _highlight.gameObject.SetActive(ColorsManager.Instance.IsActiveColorSet(colorSet));
        }

        public void SetActiveColorSet()
        {
            _controller.SetActiveColorSet(_colorSet);
        }

        public void RequestOpenSetEditor()
        {
            _controller.RequestOpenSetEditor(_colorSet);
        }
    }
}