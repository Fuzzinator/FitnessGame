using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using UI.Scrollers;
using UnityEngine;

namespace UI.Scrollers
{
    public class AvailableImagesScrollerController : ScrollerController
    {
        [SerializeField]
        private FoundAvailableImagesController _imagesController;
        private List<int> _selectedImages = new List<int>();
        public IReadOnlyCollection<int> SelectedImages => _selectedImages;
        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return Mathf.CeilToInt(CustomEnvironmentsController.ImagesInDownloadsCount * .5f);
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var targetIndex = dataIndex * 2;
            if (targetIndex < CustomEnvironmentsController.ImagesInDownloadsCount)
            {
                var cellView = base.GetCellView(scroller, targetIndex, cellIndex) as AvaillableImagesCellView;
                cellView.SetData(targetIndex, this);
                return cellView;
            }
            return null;
        }

        public bool IsSelected(int index)
        {
            return _selectedImages.Contains(index);
        }

        public bool SelectImage(int index)
        {
            if (_selectedImages.Contains(index))
            {
                _selectedImages.Remove(index);
                return false;
            }
            else
            {
                _selectedImages.Add(index);
                return true;
            }
        }

        public override void Refresh()
        {
            ClearSelected();
            base.Refresh();
        }

        public void ClearSelected()
        {
            _selectedImages.Clear();
        }
    }
}