using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;

namespace UI.Scrollers.Playlists
{
    public class SongDifficultyCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private TextMeshProUGUI _difficultyName;

        private SongDifficultyScrollerController _controller;

        public void SetData(DifficultyInfo info, SongDifficultyScrollerController scroller)
        {
            _difficultyName.SetText(info.Difficulty);
            _controller = scroller;
        }

        public void DifficultySelected()
        {
            _controller.SetInfoSelected(_difficultyName.text);
        }
    }
}