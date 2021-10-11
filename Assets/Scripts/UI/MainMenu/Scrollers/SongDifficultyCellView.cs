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
        private DifficultyInfo _info;

        public void SetData(DifficultyInfo info, SongDifficultyScrollerController scroller)
        {
            _info = info;
            _difficultyName.SetText(info.Difficulty);
            _controller = scroller;
        }

        public void DifficultySelected()
        {
            _controller.SetInfoSelected(_info);
        }
    }
}