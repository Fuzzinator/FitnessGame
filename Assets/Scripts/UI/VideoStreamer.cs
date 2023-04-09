using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Video;

public class VideoStreamer : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer _videoPlayer;

    [SerializeField]
    private AssetReference[] _availableVideos;

    private AssetReference _assetRef;
    private AsyncOperationHandle<VideoClip> _handle;
    private CancellationToken _destroyedToken;
    private CancellationTokenSource _cancellationTokenSource;

    private void Start()
    {
        _destroyedToken = this.GetCancellationTokenOnDestroy();
    }

    public void RequestStreamVideo(int videoIndex)
    {
        StopVideo();
        _assetRef = _availableVideos[videoIndex];
        StreamVideo(_assetRef).Forget();
    }

    public void StopVideo()
    {
        _videoPlayer.Stop();
        if(_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
        }
        if(_handle.IsValid())
        {
            Addressables.Release(_handle);
        }
    }

    private async UniTaskVoid StreamVideo(AssetReference video)
    {
        if(_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Dispose();
        }
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_destroyedToken);
        var handle =  Addressables.LoadAssetAsync<VideoClip>(video);
        _handle = handle;
        var clip = await handle.WithCancellation(_cancellationTokenSource.Token);

        if(_cancellationTokenSource.IsCancellationRequested || _assetRef != video)
        {
            Addressables.Release(handle);
            return;
        }

        if(clip == null)
        {
            return;
        }
        _videoPlayer.clip = clip;
        _videoPlayer.Play();
    }
}
