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
            return LocalMP3sManager.AvailableMP3Paths.Count;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var mp3Name = LocalMP3sManager.GetMp3Name(dataIndex);

            var cellView = base.GetCellView(scroller, dataIndex, cellIndex) as LocalMP3CellView;
            if (LocalMP3sManager.TryGetMP3Info(dataIndex, out var file))
            {

                cellView.SetData(mp3Name, file, dataIndex, this);
            }
            else
            {
                cellView.SetFallbackData(mp3Name, dataIndex, this);
            }
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