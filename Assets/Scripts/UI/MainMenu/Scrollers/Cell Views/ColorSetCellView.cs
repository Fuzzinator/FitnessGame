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

        [SerializeField]
        private Button _editButton;
        
        private ColorsManager.ColorSet _colorSet;
        private int _index;
        private AvailableColorSetsScrollerController _controller;

        public void SetData(ColorsManager.ColorSet colorSet,int index, AvailableColorSetsScrollerController controller)
        {
            _colorSet = colorSet;
            _leftColor.color = colorSet.LeftController;
            _rightColor.color = colorSet.RightController;
            _blockColor.color = colorSet.BlockColor;
            _obstacleColor.color = colorSet.ObstacleColor;
            SetHighlight(ColorsManager.Instance.ActiveSetIndex == index);
            
            _index = index;
            _controller = controller;
            _editButton.gameObject.SetActive(index != 0);
        }

        public void SetHighlight(bool on)
        {
            _highlight.gameObject.SetActive(on);
        }

        public void SetActiveColorSet()
        {
            _controller.SetActiveColorSet(_colorSet, _index);
            _controller.SetHighlight(this);
        }

        public void RequestOpenSetEditor()
        {
            _controller.RequestOpenSetEditor(_colorSet, _index);
        }
    }
}