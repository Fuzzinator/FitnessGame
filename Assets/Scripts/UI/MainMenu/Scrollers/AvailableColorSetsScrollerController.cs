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
            return ColorsManager.Instance.AvailableColorSets.Count;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = base.GetCellView(scroller, dataIndex, cellIndex) as ColorSetCellView;
            cellView.SetData(ColorsManager.Instance.AvailableColorSets[dataIndex], dataIndex-1, this);
            return cellView;
        }

        public virtual void SetActiveColorSet(ColorSet info, int index)
        {
            _colorSetSelector.SetActiveColorSet(info, index+1);
        }

        public void SetHighlight(ColorSetCellView cellView)
        {
            foreach (var enhancedScrollerCellView in _scroller.ActiveCells.data)
            {
                var cell = (ColorSetCellView) enhancedScrollerCellView;
                if (cell == null)
                {
                    continue;
                }

                cell.SetHighlight(cell == cellView);
            }
        }

        public void RequestOpenSetEditor(ColorSet set, int index)
        {
            _colorSetSelector.RequestOpenSetEditor(set, index);
        }
    }
}