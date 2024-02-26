using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UI.Scrollers;
using UnityEngine;
using UnityEngine.Pool;

public class AvailableLocalMP3sUIController : MonoBehaviour
{
    [SerializeField]
    private LoadingDisplaysController _loadingDisplays;
    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField]
    private AvailableLocalMp3sScrollerController _scroller;

    private bool _initialized = false;
    private bool _loadingSongPreview = false;
    private int _previewingIndex = -1;
    private CancellationToken _cancellationToken;
    private CancellationTokenSource _cancellationSource;

    public List<string> AvailableMP3Paths { get; private set; }

    private readonly string[] _mp3Extension = { ".mp3" };

    private void OnEnable()
    {
        if (!_initialized)
        {
            Initialize();
        }
        DisplayBeatsageInformation();
        UpdateAvailableMp3s();
        DisplayActiveDownloads();
    }

    private void OnDisable()
    {
        _loadingDisplays.CancelAll(true);
        StopSongPreview();
    }

    public void Initialize()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
        _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
    }

    public void UpdateAvailableMp3s()
    {
        if (AvailableMP3Paths == null)
        {
            AvailableMP3Paths = CollectionPool<List<string>, string>.Get();
        }
        else
        {
            AvailableMP3Paths.Clear();
        }
        AssetManager.GetAssetPathsFromDownloads(_mp3Extension, AvailableMP3Paths);
        _scroller.Refresh();
    }

    public TagLib.File GetMP3Info(int index)
    {
        var filePath = AvailableMP3Paths[index];
        var file = TagLib.File.Create(filePath);
        return file;
    }

    public void TryConvertSong(int index)
    {
        var path = AvailableMP3Paths[index];
        var songName = Path.GetFileName(path);
        var download = BeatSageDownloadManager.TryAddDownload(songName, path);
        if(download == null)
        {
            return;
        }
        var loadingDisplay = _loadingDisplays.DisplayNewLoading(songName);
        loadingDisplay.SetUpBeatsageDownload(download);
    }

    public void ToggleIfSameSong(int index)
    {
        if (_loadingSongPreview || _audioSource.isPlaying)
        {
            StopSongPreview();
        }
        if (_previewingIndex == index)
        {
            return;
        }
        _previewingIndex = index;
        PlaySongAudioAsync(index).Forget();
    }
    private async UniTaskVoid RefreshTokenAsync()
    {
        _cancellationSource?.Cancel();
        await UniTask.DelayFrame(1);
        _cancellationSource?.Dispose();
        _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
    }

    public void StopSongPreview()
    {
        _audioSource.Stop();
        if (_cancellationSource != null && _cancellationSource.IsCancellationRequested)
        {
            _cancellationSource.Dispose();
            _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
        }
        else
        {
            RefreshTokenAsync().Forget();
        }
        _loadingSongPreview = false;
    }
    private async UniTaskVoid PlaySongAudioAsync(int index)
    {
        AudioClip audioClip = null;
        if (_cancellationSource.IsCancellationRequested)
        {
            _cancellationSource.Dispose();
            _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
        }

        _loadingSongPreview = true;

        var path = AvailableMP3Paths[index];
        audioClip = await AssetManager.LoadCustomSong($"file://{path}", _cancellationSource.Token, AudioType.MPEG, false);


        await UniTask.DelayFrame(1, cancellationToken: _cancellationSource.Token).SuppressCancellationThrow();
        if (_cancellationSource.IsCancellationRequested)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                return;
            }
            StopSongPreview();
            return;
        }
        _audioSource.clip = audioClip;
        _audioSource.Play();

        _loadingSongPreview = false;
        await UniTask.WaitUntil(() => _audioSource.isPlaying, cancellationToken: _cancellationSource.Token);

        if (_cancellationSource.IsCancellationRequested)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                return;
            }
            StopSongPreview();
            return;
        }

        await UniTask.WaitUntil(() => !_audioSource.isPlaying && FocusTracker.Instance.IsFocused, cancellationToken: _cancellationSource.Token);

        if (_cancellationToken.IsCancellationRequested)
        {
            return;
        }

        StopSongPreview();
    }

    private void DisplayActiveDownloads()
    {
        foreach (var download in BeatSageDownloadManager.Downloads)
        {
            if (download == null)
            {
                continue;
            }
            var loadingDisplay = _loadingDisplays.DisplayNewLoading(download.FileName);
            loadingDisplay.SetUpBeatsageDownload(download);
        }
    }

    private void CancelDownloads()
    {
        _loadingDisplays.CancelAll();
    }

    public void GainedNetworkConnection()
    {
        RefreshTokenAsync().Forget();
    }

    public void NetworkConnectionLost()
    {
        _cancellationSource.Cancel();
        CancelDownloads();
        if (gameObject.activeInHierarchy)
        {
            MainMenuUIController.Instance.SetActivePage(0);
        }
    }

    private void DisplayBeatsageInformation()
    {
#if UNITY_ANDROID
        var visuals = new Notification.NotificationVisuals("Converting custom songs is currently powered by the free online AI tool Beat Sage. Beat Sage was made with love by Chris Donahue and Abhay Agarwal. If you love the maps generated by them, consider supporting them on patreon!", "Powered by BeatSage", "Continue", "View Patreon");
        NotificationManager.RequestNotification(visuals, null, () => Application.OpenURL("https://www.patreon.com/beatsage"));
#else
        var visuals = new Notification.NotificationVisuals("Converting custom songs is currently powered by the free online AI tool Beat Sage. Beat Sage was made with love by Chris Donahue and Abhay Agarwal. If you love the maps generated by them, consider supporting them on patreon!", "Powered by BeatSage", "Continue");
        NotificationManager.RequestNotification(visuals, null));
#endif
    }
}
