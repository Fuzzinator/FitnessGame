using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using GameModeManagement;
using UnityEngine;
using StringComparison = System.StringComparison;

namespace UI.Scrollers.Playlists
{
    public class AvailableSongInfoScrollerController : ScrollerController
    {
        [SerializeField]
        private DisplaySongInfo _displaySongInfo;

        private List<SongInfo> _songInfos = new List<SongInfo>();

        public CancellationToken CancellationToken { get; private set; }

        protected override void Start()
        {
            CancellationToken = gameObject.GetCancellationTokenOnDestroy();
            base.Start();
        }

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

        public void QuickAddToPlaylist(SongInfo info)
        {
            var playlistItem = PlaylistMaker.GetPlaylistItem(info, DifficultyInfo.DifficultyEnum.Unset.Readable(), DifficultyInfo.DifficultyEnum.Unset, GameMode.Unset);
            PlaylistMaker.Instance.AddPlaylistItem(playlistItem);
        }

        public virtual void SetActiveInfo(SongInfo info)
        {
            _displaySongInfo.RequestDisplay(info);
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
                foreach (var songInfo in /*System.Runtime.InteropServices.CollectionsMarshal.AsSpan(*/SongInfoFilesReader.Instance.availableSongs)
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

        public void ScrollToData(SongInfo info)
        {
            if(_songInfos.Count == 0)
            {
                _songInfos.AddRange(SongInfoFilesReader.Instance.availableSongs);
            }
            var dataIndex = _songInfos.IndexOf(info);
            if (dataIndex < 0)
            {
                return;
            }
            _scroller.GetScrollPositionForDataIndex(dataIndex, EnhancedScroller.CellViewPositionEnum.After);
            _scroller.SetScrollPositionImmediately(dataIndex);
        }

        public void ToggleSongPreview()
        {
            _displaySongInfo.ToggleSongPreview();
        }
    }
}