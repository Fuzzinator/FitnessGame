using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UI.Scrollers
{
    public class AvailableLocalMp3sScrollerController : ScrollerController
    {
        [SerializeField]
        private AvailableLocalMP3sUIController _availableLocalMP3sUIController;

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _availableLocalMP3sUIController.AvailableMP3Paths.Count;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var mp3Name = _availableLocalMP3sUIController.GetMp3Name(dataIndex);

            var cellView = base.GetCellView(scroller, dataIndex, cellIndex) as LocalMP3CellView;
            cellView.SetData(mp3Name, _availableLocalMP3sUIController.GetMP3Info(dataIndex), dataIndex, this);
            return cellView;
        }

        public void TryConvertSong(int index)
        {
            _availableLocalMP3sUIController.TryConvertSong(index);
        }

        public void ToggleSongPreview(int index)
        {
            _availableLocalMP3sUIController.ToggleIfSameSong(index);
        }
    }
}