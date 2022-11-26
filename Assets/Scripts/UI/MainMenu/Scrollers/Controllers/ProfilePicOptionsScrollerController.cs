using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UI.Scrollers
{
    public class ProfilePicOptionsScrollerController : ScrollerController
    {
        [SerializeField]
        private ProfileEditor _profileEditor;
        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ProfileManager.Instance?.ProfileIcons?.Count ?? 0;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = base.GetCellView(scroller, dataIndex, cellIndex) as ProfilePicCellView;
            cellView.SetData(ProfileManager.Instance.ProfileIcons[dataIndex], this);
            return cellView;
        }

        public void SetSelectedIcon(Sprite sprite)
        {
            var info = ProfileManager.Instance.TryGetInfoFromSprite(sprite);
            _profileEditor.NewIconSelected(info, sprite);
        }
    }
}