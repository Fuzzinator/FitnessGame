using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using StringComparison = System.StringComparison;

namespace UI.Scrollers.Playlists
{
    public class AvailablePlaylistsScrollerController : ScrollerController
    {
        protected List<Playlist> _playlists = new List<Playlist>();

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            if (string.IsNullOrWhiteSpace(_searchKey) && _playlists.Count == 0 || _playlists.Count == 0)
            {
                SetDataFromFilter();
            }
            return _playlists.Count;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = base.GetCellView(scroller, dataIndex, cellIndex) as PlaylistCellView;
            cellView.SetData(_playlists[dataIndex]);

            return cellView;
        }
        
        protected override void SetDataFromFilter()
        {
            _playlists.Clear();
            
            if (PlaylistFilesReader.Instance.availablePlaylists == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_searchKey))
            {
                foreach (var playlist in PlaylistFilesReader.Instance.availablePlaylists)
                {
                    if (playlist.IsHidden && !_showHiddenAssets)
                    {
                        continue;
                    }

                    _playlists.Add(playlist);
                }
            }
            else
            {
                foreach (var playlist in PlaylistFilesReader.Instance.availablePlaylists)
                {
                    if (playlist.PlaylistName.Contains(_searchKey, StringComparison.InvariantCultureIgnoreCase) ||
                        (string.Equals(_searchKey, "custom", StringComparison.InvariantCultureIgnoreCase) &&
                         playlist.IsCustomPlaylist))
                    {
                        if (playlist.IsHidden && !_showHiddenAssets)
                        {
                            continue;
                        }

                        _playlists.Add(playlist);
                    }
                }
            }
        }
    }
}