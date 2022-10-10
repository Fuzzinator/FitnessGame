using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using GameModeManagement;
using TMPro;
using UnityEngine;

namespace UI.Scrollers.Playlists
{
    public class SongDifficultyCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private TextMeshProUGUI _difficultyName;

        private SongDifficultyScrollerController _controller;
        private DifficultyInfo _info;
        private GameMode _gameMode;

        public void SetData(GameMode gameMode, DifficultyInfo info, SongDifficultyScrollerController scroller)
        {
            _gameMode = gameMode;
            _info = info;
            _difficultyName.SetText($"{gameMode.Readable()} - {info.Difficulty}");
            _controller = scroller;
        }

        public void DifficultySelected()
        {
            _controller.SetInfoSelected(_gameMode, _info);
        }
    }
}