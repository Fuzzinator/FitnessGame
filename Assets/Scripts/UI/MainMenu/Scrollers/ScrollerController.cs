using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UI.Scrollers
{
    public class ScrollerController : MonoBehaviour, IEnhancedScrollerDelegate
    {
        #if UNITY_EDITOR
        public string scriptIdentifier;
        #endif
        [SerializeField]
        protected EnhancedScroller _scroller;

        [SerializeField]
        protected EnhancedScrollerCellView _cellViewPrefab;

        [SerializeField]
        private float _cellViewSize = 100f;

        protected string _searchKey = string.Empty;

        protected virtual void Start()
        {
            if(_scroller == null)
            {
                return;
            }
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


        public virtual void SetData()
        {
            _searchKey = string.Empty;
            SetDataFromFilter();
        }
        
        public virtual void SetSearchKey(string searchKey)
        {
            _searchKey = searchKey;
            Refresh();
        }

        public virtual void Refresh()
        {
            if (_scroller.Initialized)
            {
                SetDataFromFilter();
                _scroller.ReloadData();
            }
        }

        protected virtual void SetDataFromFilter()
        {
            
        }
    }
}