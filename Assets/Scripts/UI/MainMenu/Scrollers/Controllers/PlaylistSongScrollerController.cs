using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UI.Scrollers.Playlists
{
    public class PlaylistSongScrollerController : ScrollerController
    {
        public bool highlightActiveItem = false;

        private CancellationToken _cancellationToken;
        
        protected override void Start()
        {
            _cancellationToken = gameObject.GetCancellationTokenOnDestroy();
            base.Start();
        }

        private void OnEnable()
        {
            if (highlightActiveItem)
            {
                PlaylistManager.Instance.playlistItemUpdated.AddListener(UpdateCellViewHighlight);
            }
            PlaylistManager.Instance.currentPlaylistUpdated.AddListener(Refresh);
        }

        private void OnDisable()
        {
            if (highlightActiveItem)
            {
                PlaylistManager.Instance.playlistItemUpdated.RemoveListener(UpdateCellViewHighlight);
            }
            PlaylistManager.Instance.currentPlaylistUpdated.RemoveListener(Refresh);
        }

        private void Refresh(Playlist playlist)
        {
            Refresh();
        }
        
        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            if (PlaylistManager.Instance != null)
            {
                return PlaylistManager.Instance.CurrentPlaylist?.Items?.Length ?? 0;
            }
            return 0;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = base.GetCellView(scroller, dataIndex, cellIndex);
            if (cellView is HighlightableCellView highlightableCellView)
            {
                highlightableCellView.SetData(PlaylistManager.Instance.CurrentPlaylist.Items[dataIndex]);
                highlightableCellView.CancellationToken = _cancellationToken;
                if (highlightActiveItem && dataIndex == 0)
                {
                    if (PlaylistManager.Instance.CurrentIndex == 0)
                    {
                        highlightableCellView.SetHighlight(true);
                    }
                }
            }

            return cellView;
        }

        public void UpdateCellViewHighlight(PlaylistItem item)
        {
            for (var i = 0; i < PlaylistManager.Instance.SongCount; i++)
            {
                var cellView = _scroller.GetCellViewAtDataIndex(i);
                if (cellView is HighlightableCellView view)
                {
                    view.SetHighlight(i == PlaylistManager.Instance.CurrentIndex);
                }
            }

            _scroller.SetScrollPositionImmediately(_scroller.GetScrollPositionForDataIndex(PlaylistManager.Instance.CurrentIndex,
                EnhancedScroller.CellViewPositionEnum.Before));
        }

        public void ReloadScroller()
        {
            _scroller.ReloadData();
        }
    }
}