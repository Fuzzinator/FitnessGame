using EnhancedUI.EnhancedScroller;
using UI.Scrollers.Playlists;
using UnityEngine;

namespace UI.Scrollers
{
    public class AvailableCustomEnvironmentsScrollerController : ScrollerController
    {
        [SerializeField]
        private AvailableEnvironmentsUIController _availableEnvironmentsUIController;

        [SerializeField]
        private DisplayActiveEnvironment _activeEnvironmentDisplay;

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return CustomEnvironmentsController.CustomEnvironmentCount;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = base.GetCellView(scroller, dataIndex, cellIndex) as CustomEnvironmentCellView;
            cellView.SetData(CustomEnvironmentsController.GetCustomEnvironment(dataIndex), dataIndex, this);
            return cellView;
        }

        public virtual void SetActiveEnvironment(int environment)
        {
            _activeEnvironmentDisplay.SetActiveCustomEnvironment(environment);
        }

        public virtual void EditEnvironment(int envIndex)
        {
            var environment = CustomEnvironmentsController.GetCustomEnvironment(envIndex);
            _availableEnvironmentsUIController.EditEnvironment(environment);
        }
    }
}