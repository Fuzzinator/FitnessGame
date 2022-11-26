using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Scrollers
{
    public class ProfilePicCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private Image _spriteHolder;

        private ProfilePicOptionsScrollerController _controller;

        public void SetData(Sprite sprite, ProfilePicOptionsScrollerController controller)
        {
            _spriteHolder.sprite = sprite;
            _controller = controller;
        }

        public void SetSelected()
        {
            _controller.SetSelectedIcon(_spriteHolder.sprite);
        }
    }
}