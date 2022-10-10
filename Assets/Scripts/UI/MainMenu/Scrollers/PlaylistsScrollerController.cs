using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UI.Scrollers.Playlists
{
    public class PlaylistsScrollerController : AvailablePlaylistsScrollerController
    {
        [SerializeField]
        private TransitionController _transitionController;

        [SerializeField]
        private MainMenuUIController _uiController;

        [SerializeField]
        private int _viewPlaylistPageIndex;
        
        private int _offset;
        
        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return Mathf.CeilToInt(base.GetNumberOfCells(scroller)*.5f);
        }
        
        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            if (dataIndex == 0)
            {
                _offset = 0;
            }
            if(dataIndex+_offset<_playlists.Count)
            {
                var cellView = scroller.GetCellView(_cellViewPrefab) as DualPlaylistDisplayCellView;
                var playlist1 = _playlists[dataIndex + _offset];
                _offset++;
                var playlist2 = dataIndex+_offset<_playlists.Count?_playlists[dataIndex + _offset]:new Playlist();
                cellView.SetData(playlist1, playlist2, this);

                return cellView;
            }
            Debug.LogError("Something fucked up here");
            return null;
        }

        public void PlayPlaylist()
        {
            _transitionController.RequestTransition();
            _uiController.gameObject.SetActive(false);
        }

        public void ViewPlaylist()
        {
            _uiController.SetActivePage(_viewPlaylistPageIndex);
        }
    }
}