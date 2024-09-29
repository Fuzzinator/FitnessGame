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
        
        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return Mathf.CeilToInt(base.GetNumberOfCells(scroller)*.5f);
        }
        
        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var targetIndex = dataIndex * 2;
            if(targetIndex<_playlists.Count)
            {
                var cellView = scroller.GetCellView(_cellViewPrefab) as DualPlaylistDisplayCellView;
                var playlist1 = _playlists[targetIndex];
                var playlist2 = targetIndex+1<_playlists.Count?_playlists[targetIndex+1]:null;
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
        
        protected override void SetDataFromFilter()
        {
            _playlists.Clear();
            if (string.IsNullOrWhiteSpace(_searchKey))
            {
                foreach (var playlist in PlaylistFilesReader.Instance.availablePlaylists)
                {
                    
                    if (playlist is not {isValid: true} || (playlist.IsHidden && !_showHiddenAssets))
                    {
                        continue;
                    }
                    _playlists.Add(playlist);
                }
            }
            else
            {
                foreach (var playlist in PlaylistFilesReader.Instance.availablePlaylists)
                {
                    if (playlist is not {isValid: true } || (playlist.IsHidden && !_showHiddenAssets))
                    {
                        continue;
                    }
                    if (playlist.PlaylistName.Contains(_searchKey, StringComparison.InvariantCultureIgnoreCase) ||
                        (string.Equals(_searchKey, "custom", StringComparison.InvariantCultureIgnoreCase) &&
                         playlist.IsCustomPlaylist) && playlist.isValid)
                    {
                        _playlists.Add(playlist);
                    }
                }
            }
        }
    }
}