using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using StringComparison = System.StringComparison;

namespace UI.Scrollers.Playlists
{
    public class AvailableSongInfoScrollerController : ScrollerController
    {
        [SerializeField]
        private DisplaySongInfo _displaySongInfo;

        private List<SongInfo> _songInfos = new List<SongInfo>();

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            if (string.IsNullOrWhiteSpace(_searchKey) && _songInfos.Count == 0 || _songInfos.Count == 0)
            {
                SetDataFromFilter();
            }

            return _songInfos.Count;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = base.GetCellView(scroller, dataIndex, cellIndex) as SongInfoCellView;
            cellView.SetData(_songInfos[dataIndex], this);
            return cellView;
        }

        public void SetActiveInfo(SongInfo info)
        {
            _displaySongInfo.UpdateDisplayedInfo(info);
            PlaylistMaker.Instance.SetActiveItem(info);
        }

        protected override void SetDataFromFilter()
        {
            _songInfos.Clear();
            if (string.IsNullOrWhiteSpace(_searchKey))
            {
                _songInfos.AddRange(SongInfoFilesReader.Instance.availableSongs);
            }
            else
            {
                foreach (var songInfo in SongInfoFilesReader.Instance.availableSongs)
                {
                    if (songInfo.SongName.Contains(_searchKey, StringComparison.InvariantCultureIgnoreCase) ||
                        songInfo.SongAuthorName.Contains(_searchKey, StringComparison.InvariantCultureIgnoreCase) ||
                        songInfo.LevelAuthorName.Contains(_searchKey, StringComparison.InvariantCultureIgnoreCase) ||
                        (string.Equals(_searchKey, "custom", StringComparison.InvariantCultureIgnoreCase) &&
                         songInfo.isCustomSong))
                    {
                        _songInfos.Add(songInfo);
                    }
                }
            }
        }
    }
}