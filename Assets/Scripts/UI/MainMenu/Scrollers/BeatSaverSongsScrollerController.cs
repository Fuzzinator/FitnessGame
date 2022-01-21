using System;
using System.Collections;
using System.Collections.Generic;
using BeatSaverSharp.Models;
using EnhancedUI.EnhancedScroller;
using UI.Scrollers.Playlists;
using UnityEngine;

namespace UI.Scrollers.BeatsaverIntegraton
{
    public class BeatSaverSongsScrollerController : ScrollerController
    {
        private BeatSaverPageController _pageController;
        private IReadOnlyList<Beatmap> _beatmaps;

        public void SetPageController(BeatSaverPageController controller)
        {
            _pageController = controller;
        }
        public void SetBeatmaps(IReadOnlyList<Beatmap> beatmaps)
        {
            _beatmaps = beatmaps;
            _scroller.ReloadData();
            //SetDataFromFilter();
        }

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _beatmaps?.Count??0;
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
    }
}