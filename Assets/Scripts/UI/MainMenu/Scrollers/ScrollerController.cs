using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UI.Scrollers.Playlists
{
    public class ScrollerController : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField]
        protected EnhancedScroller _scroller;

        [SerializeField]
        private EnhancedScrollerCellView _cellViewPrefab;

        [SerializeField]
        private float _cellViewSize = 100f;

        private void Start()
        {
            _scroller.Delegate = this;
            //_scroller.ReloadData();
        }

        public virtual int GetNumberOfCells(EnhancedScroller scroller)
        {
            return 0;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _cellViewSize;
        }

        public virtual EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_cellViewPrefab);
            return cellView;
        }

        public virtual void Refresh()
        {
            if (_scroller.Initialized)
            {
                _scroller.ReloadData();
            }
        }
    }
}