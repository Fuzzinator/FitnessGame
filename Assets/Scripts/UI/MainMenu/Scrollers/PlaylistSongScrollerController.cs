using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UI.Scrollers.Playlists
{
    public class PlaylistSongScrollerController : ScrollerController
    {
        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return PlaylistManager.Instance?.CurrentPlaylist.Items.Length??0;
        }
        
        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = base.GetCellView(scroller,dataIndex,cellIndex) as PlaylistSongCellView;
            cellView.SetData(PlaylistManager.Instance.CurrentPlaylist.Items[dataIndex]);
            return cellView;
        }

        public void ReloadScroller()
        {
            _scroller.ReloadData();
        }
    }
}