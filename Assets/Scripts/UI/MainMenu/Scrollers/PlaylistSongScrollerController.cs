using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UI.Scrollers.Playlists
{
    public class PlaylistSongScrollerController : ScrollerController
    {
        public bool highlightActiveItem = false;

        private void OnEnable()
        {
            if (highlightActiveItem)
            {
                PlaylistManager.Instance.playlistItemUpdated.AddListener(UpdateCellViewHighlight);
            }
        }

        private void OnDisable()
        {
            if (highlightActiveItem)
            {
                PlaylistManager.Instance.playlistItemUpdated.RemoveListener(UpdateCellViewHighlight);
            }
        }

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return PlaylistManager.Instance?.CurrentPlaylist.Items.Length ?? 0;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = base.GetCellView(scroller, dataIndex, cellIndex) as PlaylistSongCellView;
            cellView.SetData(PlaylistManager.Instance.CurrentPlaylist.Items[dataIndex]);
            if (highlightActiveItem && dataIndex == 0)
            {
                if (PlaylistManager.Instance.CurrentIndex == 0)
                {
                    cellView.SetHighlight(true);
                }
            }

            return cellView;
        }

        public void UpdateCellViewHighlight(PlaylistItem item)
        {
            for (var i = 0; i < PlaylistManager.Instance.SongCount; i++)
            {
                var cellView = _scroller.GetCellViewAtDataIndex(i);
                if (cellView is PlaylistSongCellView view)
                {
                    view.SetHighlight(i == PlaylistManager.Instance.CurrentIndex);
                }
            }

            _scroller.ReloadData(
                _scroller.GetScrollPositionForDataIndex(PlaylistManager.Instance.CurrentIndex,
                    EnhancedScroller.CellViewPositionEnum.Before));
        }

        public void ReloadScroller()
        {
            _scroller.ReloadData();
        }
    }
}