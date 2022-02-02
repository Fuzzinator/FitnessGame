using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using GameModeManagement;
using UnityEngine;

namespace UI.Scrollers.Playlists
{
    public class SongDifficultyScrollerController : ScrollerController
    {
        private SongInfo _songInfo;
        private SongInfo.DifficultySet[] _difficultySet;
        private GameMode _selectedGameMode;

        public void UpdateDifficultyOptions(SongInfo songInfo, SongInfo.DifficultySet[] difficultyInfo, GameMode gameMode)
        {
            _songInfo = songInfo;
            _difficultySet = difficultyInfo;
            _selectedGameMode = gameMode;
            _scroller.ReloadData();
        }

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            var count = 0;
            if (_difficultySet == null)
            {
                return count;
            }
            foreach (var set in _difficultySet)
            {
                count += set.DifficultyInfos.Length;
            }

            return count;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = base.GetCellView(scroller, dataIndex, cellIndex) as SongDifficultyCellView;
            var count = 0;
            while (dataIndex - _difficultySet[count].DifficultyInfos.Length >= 0)
            {
                dataIndex -= _difficultySet[count].DifficultyInfos.Length;
                count++;
            }
            cellView.SetData(_difficultySet[count].BeatMapName.GetGameMode(), _difficultySet[count].DifficultyInfos[dataIndex], this);
            return cellView;
        }

        public void SetInfoSelected(GameMode gameMode, DifficultyInfo difficulty)
        {
            if (PlaylistMaker.Instance != null)
            {
                var playlistItem = PlaylistMaker.GetPlaylistItem(_songInfo, difficulty.Difficulty, gameMode);
                PlaylistMaker.Instance.AppendPlaylistItems(playlistItem);
            }
        }
    }
}