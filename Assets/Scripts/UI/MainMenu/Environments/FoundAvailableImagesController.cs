using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UI.Scrollers;
using UnityEngine;

public class FoundAvailableImagesController : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup _myCanvasController;
    [SerializeField]
    private SkyboxTextureController _myTextureController;
    [SerializeField]
    private AvailableImagesScrollerController _scrollerController;

    private CancellationToken _cancellationToken;

    private void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    public void StartFindingImages()
    {
        gameObject.SetActive(true);
        _myCanvasController.SetGroupState(1, true);
        RefreshImages();
    }

    public void AddSelected()
    {
        foreach (var index in _scrollerController.SelectedImages)
        {
            var path = CustomEnvironmentsController.GetDownloadsImagePath(index);
            var success = CustomEnvironmentsController.TrySetImageAsSkybox(path, null, true, _cancellationToken);
        }
        Cancel();
    }

    public void RefreshImages()
    {
        CustomEnvironmentsController.GetImagePathsInDownloads();
        _scrollerController.Refresh();
    }

    public void Cancel()
    {
        _scrollerController.ClearSelected();
        gameObject.SetActive(false);
        _myTextureController.CompleteGetMoreSkyboxes();
    }
}
