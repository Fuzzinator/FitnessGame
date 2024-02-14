using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UI.Scrollers.Playlists
{
    public class PlaylistSongInfoScrollerController : ScrollerController
    {
        [SerializeField]
        private DisplaySongInfo _displaySongInfo;

        [field: SerializeField]
        public RectTransform CellViewParent { get; private set;}

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return PlaylistMaker.Instance?.PlaylistItems.Count??0;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = base.GetCellView(scroller, dataIndex, cellIndex) as PlaylistSongInfoCellView;
            cellView.SetData(this, PlaylistMaker.Instance.PlaylistItems[dataIndex], dataIndex);
            return cellView;
        }

        public virtual void SetActiveInfo(SongInfo info, int dataIndex)
        {
            _displaySongInfo.RequestDisplay(info);
            PlaylistMaker.Instance.SetActiveItem(info);
            PlaylistMaker.Instance.SetActivePlaylistItem(dataIndex);
        }

        public void SetNewIndex(int startingIndex, int offset)
        {
            var item = PlaylistMaker.Instance.PlaylistItems[startingIndex];
            PlaylistMaker.Instance.PlaylistItems.RemoveAt(startingIndex);
            var newIndex = Mathf.Clamp(startingIndex + offset, 0, PlaylistMaker.Instance.PlaylistItems.Count);
            PlaylistMaker.Instance.PlaylistItems.Insert(newIndex, item);
            var scrollPos = 1 - _scroller.ScrollRect.verticalNormalizedPosition;
            Refresh(scrollPos);
        }
    }
}