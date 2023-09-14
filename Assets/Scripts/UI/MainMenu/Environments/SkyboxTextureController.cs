using System.Collections;
using System.Collections.Generic;
using UI.Scrollers;
using UnityEngine;

public class SkyboxTextureController : MonoBehaviour
{
    [SerializeField]
    private CustomEnvironmentCreator _environmentController;
    [SerializeField]
    private CanvasGroup _myCanvasController;
    [SerializeField]
    private CustomSkyboxesScrollerController _skyboxScrollerController;
    [SerializeField]
    private FoundAvailableImagesController _foundAvailableImagesController;

    private bool _depthTexture = false;

    public void StartEditSkyboxTexture()
    {
        _depthTexture = false;
        Enable();
    }

    public void StartEditSkyboxDepthTexture()
    {
        _depthTexture = true; 
        Enable();
    }

    private void Enable()
    {
        gameObject.SetActive(true);
        _myCanvasController.SetGroupState(1, true);
        var skyboxes = CustomEnvironmentsController.GetAvailableSkyboxes(_depthTexture);
        _skyboxScrollerController.Refresh();

        if (skyboxes.Count == 0)
        {
            var visuals = new Notification.NotificationVisuals("No Skyboxes found, please select the + button to look through your downloads folder for panoramic skyboxes.", "No Skyboxes", "Okay", disableUI : true);
            NotificationManager.RequestNotification(visuals);
        }
    }

    public void SetSelectedSkybox(int index, Sprite thumbnail)
    {
        if (_depthTexture)
        {
            var skyboxName = CustomEnvironmentsController.GetSkyboxDepthName(index);
            var skyboxPath = CustomEnvironmentsController.GetSkyboxDepthPath(index);
            _environmentController.SetSkyboxTexture(skyboxName, skyboxPath, thumbnail);
        }
        else
        {
            var skyboxName = CustomEnvironmentsController.GetSkyboxName(index);
            var skyboxPath = CustomEnvironmentsController.GetSkyboxPath(index);
            _environmentController.SetSkyboxTexture(skyboxName, skyboxPath, thumbnail);
        }
        CompleteSetSkybox();
    }


    public void GetMoreSkyboxes()
    {
        _myCanvasController.SetGroupState(1, false);
        _foundAvailableImagesController.StartFindingImages();
    }

    public void CompleteGetMoreSkyboxes()
    {
        _myCanvasController.SetGroupState(1, true);
        _skyboxScrollerController.Refresh();
    }

    public void CompleteSetSkybox()
    {
        gameObject.SetActive(false);
        _environmentController.CompleteSetSkybox();
        _depthTexture = false;
    }

    public void RenameSkybox(string newName)
    {
        _environmentController.RenameSkybox(newName);
    }

    public void DeleteSkybox(string skyboxName)
    {
        _environmentController.CheckSkyboxDeleted(skyboxName, _depthTexture);
    }
}
