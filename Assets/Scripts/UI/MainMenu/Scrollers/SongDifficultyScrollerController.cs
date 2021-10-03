using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UI.Scrollers.Playlists
{
    public class SongDifficultyScrollerController : ScrollerController
    {
        private SongInfo.DifficultySet _difficultyInfo;

        public void UpdateDifficultyOptions(SongInfo.DifficultySet info)
        {
            _difficultyInfo = info;
            _scroller.ReloadData();
        }

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _difficultyInfo.DifficultyInfos?.Length ?? 0;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = base.GetCellView(scroller, dataIndex, cellIndex) as SongDifficultyCellView;
            cellView.SetData(_difficultyInfo.DifficultyInfos[dataIndex], this);
            return cellView;
        }

        public void SetInfoSelected(string difficulty)
        {
            if (PlaylistMaker.Instance != null)
            {
                PlaylistMaker.Instance.AppendPlaylistItems(PlaylistMaker.Instance.GetPlaylistItem(difficulty));
            }
        }
    }
}