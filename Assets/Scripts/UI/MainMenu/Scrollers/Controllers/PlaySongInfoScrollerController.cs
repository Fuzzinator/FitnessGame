using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using StringComparison = System.StringComparison;

namespace UI.Scrollers.Playlists
{
    public class PlaySongInfoScrollerController : AvailableSongInfoScrollerController
    {
        [SerializeField]
        private DisplaySongRecords _songRecordsDisplay;

        public override void SetActiveInfo(SongInfo info)
        {
            _songRecordsDisplay.SetSongInfo(info);
            base.SetActiveInfo(info);
        }
    }
}