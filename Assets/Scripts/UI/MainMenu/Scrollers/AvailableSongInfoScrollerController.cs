using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UI.Scrollers.Playlists
{
    public class AvailableSongInfoScrollerController : ScrollerController
    {
        [SerializeField]
        private DisplaySongInfo _displaySongInfo;
        
        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return SongInfoFilesReader.Instance.availableSongs.Count;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView =  base.GetCellView(scroller, dataIndex, cellIndex) as SongInfoCellView;
            cellView.SetData(SongInfoFilesReader.Instance.availableSongs[dataIndex], this);
            return cellView;
        }

        public void SetActiveInfo(SongInfo info)
        {
            _displaySongInfo.UpdateDisplayedInfo(info);
            PlaylistMaker.Instance.SetActiveItem(info);
        }
    }
}