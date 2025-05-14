using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

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

        [SerializeField]
        private MultiColorButton _scrollUpButton;

        [SerializeField]
        private MultiColorButton _scrollDownButton;

        private int _cachedCellCount = 0;

        private RectTransform _container;

        private Transform _imagesHolder;
        private Transform _playButtonHolder;
        private Transform _playButtonImageHolder;
        private Transform _textHolder;

        private List<PlaylistDisplayItemHolder> _displayItemHolders = new List<PlaylistDisplayItemHolder>();

        private void SetUpContainers()
        {
            _container = _scroller.Container;
            if(_container == null)
            {
                return;
            }

            _imagesHolder = new GameObject("Images Holder").transform;
            var layoutElement = _imagesHolder.gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
            _imagesHolder.SetParent(_container, false);
            _imagesHolder.localPosition = Vector3.zero;

            _playButtonHolder = new GameObject("Play Buttons Holder").transform;
            layoutElement = _playButtonHolder.gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
            _playButtonHolder.SetParent(_container, false);
            _playButtonHolder.localPosition = Vector3.zero;

            _playButtonImageHolder = new GameObject("Play Button Images Holder").transform;
            layoutElement = _playButtonImageHolder.gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
            _playButtonImageHolder.SetParent(_container, false);
            _playButtonImageHolder.localPosition = Vector3.zero;

            _textHolder = new GameObject("Texts Holder").transform;
            layoutElement = _textHolder.gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
            _textHolder.SetParent(_container, false);
            _textHolder.localPosition = Vector3.zero;
        }

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            _cachedCellCount = Mathf.CeilToInt(base.GetNumberOfCells(scroller)*.5f);
            return _cachedCellCount;
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

                //GetItemHolders(cellView.Holder1);
                //GetItemHolders(cellView.Holder2);

                return cellView;
            }
            Debug.LogError("Something fucked up here");
            return null;
        }

        private void GetItemHolders(PlaylistDisplayItemHolder holder1)
        {
            if(holder1 != null && holder1.isActiveAndEnabled && !_displayItemHolders.Contains(holder1))
            {
                _displayItemHolders.Add(holder1);
                holder1.ImagesHolder.SetParent(_imagesHolder);
                holder1.PlayButtonHolder.SetParent(_playButtonHolder);
                holder1.PlayButtonImageHolder.SetParent(_playButtonImageHolder);
                holder1.TextHolder.SetParent(_textHolder);

                holder1.MatchPosition();
            }
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


            if (_container != null)
            {
                return;
            }

            //SetUpContainers();
        }

        public void OnScrollPositionChanged(EnhancedScroller scroller)
        {
            CheckScrollPosition(scroller);
            UpdateDisplayHolders();
        }

        private void CheckScrollPosition(EnhancedScroller scroller)
        {
            if(scroller.NumberOfCells == 0)
            {
                return;
            }
            _scrollUpButton.gameObject.SetActive(scroller.StartDataIndex != 0);
            _scrollDownButton.gameObject.SetActive(scroller.EndDataIndex != _cachedCellCount-1);

        }

        private void UpdateDisplayHolders()
        {
            foreach (var holder in _displayItemHolders)
            {
                holder.MatchPosition();
            }
        }
    }
}