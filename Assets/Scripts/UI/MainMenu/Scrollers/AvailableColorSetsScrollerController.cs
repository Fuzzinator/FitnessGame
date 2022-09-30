using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UI.Scrollers.Playlists;
using ColorSet = ColorsManager.ColorSet;
using UnityEngine;

namespace UI.Scrollers
{
    public class AvailableColorSetsScrollerController : ScrollerController
    {
        [SerializeField]
        private ColorSetSelector _colorSetSelector;
        
        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ColorsManager.Instance.AvailableColorSets.Length;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = base.GetCellView(scroller, dataIndex, cellIndex) as ColorSetCellView;
            cellView.SetData(ColorsManager.Instance.AvailableColorSets[dataIndex], this);
            return cellView;
        }

        public virtual void SetActiveColorSet(ColorSet info)
        {
            _colorSetSelector.SetActiveColorSet(info);
        }

        public void RequestOpenSetEditor(ColorSet set)
        {
            _colorSetSelector.RequestOpenSetEditor(set);
        }
    }
}