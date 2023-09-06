using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using UI.Scrollers;
using UnityEngine;

namespace UI.Scrollers
{
    public class CustomSkyboxesScrollerController : ScrollerController
    {
        [SerializeField]
        private SkyboxTextureController _skyboxTextureController;
                 
        
        public readonly Notification.NotificationVisualInfo RenameFailedVisuals = new Notification.NotificationVisualInfo
        {
            button1Txt = "Okay",
            disableUI = true,
            header = "Cannot Rename",
            message = "Cannot rename skybox because one with the same name already exists."
        };

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return Mathf.CeilToInt(CustomEnvironmentsController.CustomSkyboxesCount*.5f);
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var targetIndex = dataIndex * 2;
            if (targetIndex < CustomEnvironmentsController.CustomSkyboxesCount)
            {
                var cellView = base.GetCellView(scroller, targetIndex, cellIndex) as CustomSkyboxCellView;
                cellView.SetData(targetIndex, this);
                return cellView;
            }
            return null;
        }

        public void SelectSkybox(int index, Sprite thumbnail)
        {
            _skyboxTextureController.SetSelectedSkybox(index, thumbnail);
        }

        public void DeleteSkybox(string skyboxName)
        {
            _skyboxTextureController.DeleteSkybox(skyboxName);
        }
    }
}