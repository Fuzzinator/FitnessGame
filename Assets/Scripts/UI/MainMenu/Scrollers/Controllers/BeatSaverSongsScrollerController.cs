using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using BeatSaverSharp.Models;
using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using UI.Scrollers.Playlists;
using UnityEngine;

namespace UI.Scrollers.BeatsaverIntegraton
{
    public class BeatSaverSongsScrollerController : ScrollerController
    {
        private BeatSaverPageController _pageController;
        private IReadOnlyList<Beatmap> _beatmaps;

        public CancellationToken CancellationToken { get; private set; }

        protected override void Start()
        {
            CancellationToken = gameObject.GetCancellationTokenOnDestroy();
            base.Start();
        }

        public void SetPageController(BeatSaverPageController controller)
        {
            _pageController = controller;
        }
        public void SetBeatmaps(IReadOnlyList<Beatmap> beatmaps, float scrollPosition = 0, bool resetPageIndex = false)
        {
            _beatmaps = beatmaps;
            if (_scroller.Initialized)
            {
                var velocity = _scroller.Velocity;
                _scroller.ReloadData(scrollPosition);
                _scroller.Velocity = velocity;
                _scroller.ScrollRect.velocity = velocity;
            }
            if (resetPageIndex)
            {
                var endlessScroller = _scroller as EndlessEnhancedScroller;
                if (endlessScroller != null)
                {
                    endlessScroller.ResetPage();
                }
            }
        }

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _beatmaps?.Count ?? 0;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = base.GetCellView(scroller, dataIndex, cellIndex) as BeatSaverSongCellView;
            cellView.SetData(_beatmaps[dataIndex], this);



            return cellView;
        }

        public void SetActiveBeatmap(Beatmap beatmap)
        {
            _pageController.SetActiveBeatmap(beatmap);
        }

        public void SetSelectedCellView(BeatSaverSongCellView cellView)
        {
            _pageController.SetSelectedCellView(cellView);
        }

        public void Disable()
        {
            _scroller.ScrollRect.enabled = false;
            _scroller.Scrollbar.gameObject.SetActive(false);
        }

        public void Enable()
        {
            _scroller.ScrollRect.enabled = true;
            _scroller.Scrollbar.gameObject.SetActive(true);
        }
    }
}