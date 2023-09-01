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
        CustomEnvironmentsController.GetAvailableSkyboxes(_depthTexture);
        _skyboxScrollerController.Refresh();
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

    public void DeleteSkybox(string skyboxName)
    {
        _environmentController.CheckSkyboxDeleted(skyboxName, _depthTexture);
    }
}
