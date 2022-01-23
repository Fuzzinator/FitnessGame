using System.Collections;
using System.Collections.Generic;
using BeatSaverSharp.Models;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Scrollers.BeatsaverIntegraton
{
    public class BeatSaverSongCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private TextMeshProUGUI _songName;
        
        [SerializeField]
        private TextMeshProUGUI _songAuthor;

        [SerializeField]
        private TextMeshProUGUI _levelAuthor;

        private Beatmap _beatmap;
        private BeatSaverSongsScrollerController _controller;
        
        public void SetData(Beatmap item, BeatSaverSongsScrollerController controller)
        {
            _songName.SetText(item.Metadata.SongName);
            _songAuthor.SetText(item.Metadata.SongAuthorName);
            _levelAuthor.SetText(item.Metadata.LevelAuthorName);
            _beatmap = item;
            _controller = controller;
        }

        public void Selected()
        {
            _controller.SetActiveBeatmap(_beatmap);
        }
    }
}