using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UI.Scrollers.Playlists;
using UnityEngine;

namespace UI.Scrollers
{
    public class DualPlaylistDisplayCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private PlaylistDisplayObject _displayObject1;

        [SerializeField]
        private PlaylistDisplayObject _displayObject2;

        private PlaylistsScrollerController _controller;
        public PlaylistDisplayItemHolder Holder1 => _displayObject1 != null ? _displayObject1.ItemHolder : null;
        public PlaylistDisplayItemHolder Holder2 => _displayObject2 != null ? _displayObject2.ItemHolder : null;

        public void SetData(Playlist playlist1, Playlist playlist2, PlaylistsScrollerController controller)
        {
            _displayObject1.SetData(playlist1);
            _displayObject2.SetData(playlist2);
            _controller = controller;
        }

        public void PlayPlaylist()
        {
            _controller.PlayPlaylist();
        }

        public void ViewPlaylist()
        {
            _controller.ViewPlaylist();
        }
    }
}