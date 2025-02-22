using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using GameModeManagement;
using UnityEngine;
using static UnityEngine.XR.Hands.XRHandSubsystemDescriptor;
using StringComparison = System.StringComparison;

namespace UI.Scrollers.Playlists
{
    public class AvailableSongInfoScrollerController : ScrollerController
    {
        [SerializeField]
        private DisplaySongInfo _displaySongInfo;
        [SerializeField]
        private SetAndShowSongOptions _songOptions;

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
            var playlistItem = PlaylistMaker.GetPlaylistItem(info, DifficultyInfo.DifficultyEnum.Unset.Readable(), DifficultyInfo.DifficultyEnum.Unset, GameMode.Unset, false, false, false);
            PlaylistMaker.Instance.AddPlaylistItem(playlistItem);
        }

        public virtual void SetActiveInfo(SongInfo info)
        {
            _displaySongInfo.RequestDisplay(info);
            PlaylistMaker.Instance.SetActiveItem(info);
        }

        public void ToggleSongPlay(SongInfo info)
        {
            _displaySongInfo.ToggleSongPreview();
        }

        protected override void SetDataFromFilter()
        {
            _songInfos.Clear();
            if (string.IsNullOrWhiteSpace(_searchKey))
            {
                foreach (var songInfo in SongInfoFilesReader.Instance.filteredAvailableSongs)
                {
                    if (songInfo.IsHidden && !_showHiddenAssets)
                    {
                        continue;
                    }

                    _songInfos.Add(songInfo);
                }
            }
            else
            {
                foreach (var songInfo in SongInfoFilesReader.Instance.filteredAvailableSongs)
                {
                    if (songInfo.SongName.Contains(_searchKey, StringComparison.InvariantCultureIgnoreCase) ||
                        songInfo.SongAuthorName.Contains(_searchKey, StringComparison.InvariantCultureIgnoreCase) ||
                        songInfo.LevelAuthorName.Contains(_searchKey, StringComparison.InvariantCultureIgnoreCase) ||
                        (string.Equals(_searchKey, "custom", StringComparison.InvariantCultureIgnoreCase) &&
                         songInfo.isCustomSong))
                    {
                        if (songInfo.IsHidden && !_showHiddenAssets)
                        {
                            continue;
                        }
                        _songInfos.Add(songInfo);
                    }
                }
            }
        }

        public void ScrollToData(SongInfo info)
        {
            if(_songInfos.Count == 0)
            {
                _songInfos.AddRange(SongInfoFilesReader.Instance.filteredAvailableSongs);
            }
            var dataIndex = _songInfos.IndexOf(info);
            if (dataIndex < 0)
            {
                return;
            }
            _scroller.GetScrollPositionForDataIndex(dataIndex, EnhancedScroller.CellViewPositionEnum.After);
            _scroller.SetScrollPositionImmediately(dataIndex);
        }

        public void ToggleSongPreview(SongInfo info)
        {
            if(!_songOptions.Initialized)
            {
                _songOptions.Initialize();
            }
            _songOptions.ToggleIfSameSong(info);
        }
    }
}