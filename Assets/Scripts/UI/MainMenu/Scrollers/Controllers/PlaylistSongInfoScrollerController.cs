using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UI.Scrollers.Playlists
{
    public class PlaylistSongInfoScrollerController : ScrollerController
    {
        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return PlaylistMaker.Instance?.PlaylistItems.Count??0;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = base.GetCellView(scroller, dataIndex, cellIndex) as PlaylistSongInfoCellView;
            cellView.SetData(PlaylistMaker.Instance.PlaylistItems[dataIndex]);
            return cellView;
        }
    }
}