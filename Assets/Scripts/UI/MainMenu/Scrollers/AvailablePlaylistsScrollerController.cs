using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UI.Scrollers.Playlists
{
    public class AvailablePlaylistsScrollerController : ScrollerController
    {
        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return PlaylistFilesReader.Instance?.availablePlaylists.Count ?? 0;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = base.GetCellView(scroller, dataIndex, cellIndex) as PlaylistCellView;
            cellView.SetData(PlaylistFilesReader.Instance.availablePlaylists[dataIndex]);

            return cellView;
        }
    }
}