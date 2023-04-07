using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EnhancedUI.EnhancedScroller
{
    public class EndlessEnhancedScroller : EnhancedScroller
    {
        [SerializeField]
        private UnityEvent<float> _loopingBackwards = new UnityEvent<float>();

        [SerializeField]
        private UnityEvent<float> _loopingForward = new UnityEvent<float>();
        
        private int _currentPageNumber = 0;

        private const float TopOffset = .8f;
        private const float BottomOffset = .2f;
        private const float PlacementOffset = .02f;


        public void ResetPage()
        {
            _currentPageNumber = 0;
        }

        public void MonitorScroll(Vector2 scrollValue)
        {
            if (!_initialized || scrollValue.y == 1f)
            {
                return;
            }
            var scroll = scrollValue.y;
            if (scroll > TopOffset)
            {
                if (_currentPageNumber > 0)
                {
                    _currentPageNumber--;
                    _loopingBackwards.Invoke(TopOffset-PlacementOffset);
                }
            }
            else if (scroll < BottomOffset)
            {
                _currentPageNumber++;
                _loopingForward.Invoke(BottomOffset+ PlacementOffset);
            }
        }
    }
}