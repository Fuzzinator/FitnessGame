using System;
using System.Collections.Generic;
using System.Threading;
using BeatSaverSharp;
using BeatSaverSharp.Models;
using BeatSaverSharp.Models.Pages;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UI.Scrollers.BeatsaverIntegraton;
using UnityEngine.Serialization;

public class BeatSaverPageController : MonoBehaviour
{
    [SerializeField]
    private BeatSaverSongsScrollerController _scrollerController;

    [SerializeField]
    private GameObject _showLoadingObject;

    [SerializeField]
    private TMP_InputField _inputField;

    [Header("Info Cards")]
    [SerializeField]
    private GameObject _infoCardHolder;

    [FormerlySerializedAs("_songName")]
    [SerializeField]
    private TextMeshProUGUI _songDetails;

    [SerializeField]
    private TextMeshProUGUI _mapDifficulties;

    [SerializeField]
    private Image _songImage;

    [SerializeField]
    private Button _downloadButton;

    [SerializeField]
    private LoadingDisplaysController _loadingDisplays;

    [SerializeField]
    private AudioSource _audioSource;

    private Page _activePage;
    private Page _nextPage;

    private CancellationToken _token;
    private CancellationTokenSource _cancellationTokenSource;
    private CancellationTokenSource _downloadsTokenSource;
    private BeatSaver _beatSaver;
    private LevelFileManagement _levelFileManagement;

    private Beatmap _activeBeatmap;

    private Texture2D _activeBeatmapImage;

    private List<string> _downloadingIds = new List<string>();

    private BeatSaverSongCellView _cellView;

    private List<Beatmap> _allBeatmaps = new List<Beatmap>();

    private const int MINUTE = 60;

    private const string SONGINFOFORMAT =
        "<voffset=10>Song Name:           {0}\nSong Author:<line-indent=1>         {1}<line-indent=0>\nLevel Author:<line-indent=1>         {2}<line-indent=0>\nSong Length:         <line-indent=1>{3}:{4}<line-indent=0>\nSong Score:          <line-indent=1> {5}<line-indent=0>\nUpload Date:         <line-indent=2>{6}<line-indent=0>";

    public void Initialize()
    {
        _token = this.GetCancellationTokenOnDestroy();
        _beatSaver = new BeatSaver(Application.productName, Version.Parse(Application.version));
        _scrollerController.SetPageController(this);

        ZipFileManagement.Initialize(AssetManager.SongsPath);
        _levelFileManagement = new LevelFileManagement(AssetManager.SongsPath);
    }

    private void OnDisable()
    {
        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }

    public void RequestFilterBy(int sortingOptions)
    {
        _showLoadingObject.SetActive(true);
        var search = ((SortingOptions)sortingOptions) switch
        {
            SortingOptions.Latest => SearchTextFilterOption.Latest,
            SortingOptions.Relevance => SearchTextFilterOption.Relevance,
            SortingOptions.Rating => SearchTextFilterOption.Rating,
            SortingOptions.Curated => SearchTextFilterOption.Curated,
            _ => throw new ArgumentOutOfRangeException(nameof(sortingOptions), sortingOptions, null)
        };
        search.Query = _inputField.text;

        SearchAsync(search).Forget();
    }

    public void GainedNetworkConnection()
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Dispose();
        }
        _token = this.GetCancellationTokenOnDestroy();
    }

    public void NetworkConnectionLost()
    {
        _cancellationTokenSource.Cancel();
        CancelDownloads();
        if (gameObject.activeInHierarchy)
        {
            MainMenuUIController.Instance.SetActivePage(0);
        }
    }

    #region Webcalls

    public void RequestCurated()
    {
        _showLoadingObject.SetActive(true);
        var alphabetical = SearchTextFilterOption.Curated;
        alphabetical.Query = _inputField.text;

        SearchAsync(alphabetical).Forget();
    }

    public void RequestLatest()
    {
        _showLoadingObject.SetActive(true);
        RequestLatestAsync().Forget();
    }

    public void RequestHighestRated()
    {
        _showLoadingObject.SetActive(true);
        var rating = SearchTextFilterOption.Rating;
        rating.Query = _inputField.text;

        SearchAsync(rating).Forget();
    }

    public void RequestMostRelevant()
    {
        _showLoadingObject.SetActive(true);
        var relevance = SearchTextFilterOption.Relevance;
        relevance.Query = _inputField.text;
        SearchAsync(relevance).Forget();
    }

    public void RequestNextPage()
    {
        if (_activePage == null)
        {
            return;
        }

        _showLoadingObject.SetActive(true);
        RequestNextPageAsync().Forget();
    }

    public void RequestPreviousPage()
    {
        if (_activePage == null)
        {
            return;
        }

        _showLoadingObject.SetActive(true);
        RequestPreviousPageAsync().Forget();
    }

    public void RequestSearch(TMP_InputField textField)
    {
        _showLoadingObject.SetActive(true);
        Search(textField.text);
    }

    public void Search(string search)
    {
        if (_activePage == null)
        {
            return;
        }

        _showLoadingObject.SetActive(true);
        var options = new SearchTextFilterOption(search);
        SearchAsync(options).Forget();
    }

    public void RequestPlaySongAudio()
    {
        if (_activeBeatmap == null)
        {
            return;
        }
        PlaySongAudioAsync().Forget();
    }

    public void TryDownloadSong()
    {
        if(GameManager.Instance.DemoMode && SongInfoFilesReader.Instance.CustomSongsCount >= GameManager.DemoModeMaxCustomSongs)
        {
            var visuals = new Notification.NotificationVisuals(
                        $"Cannot download song. The maximum number of custom songs in this demo is {GameManager.DemoModeMaxCustomSongs}. To download more custom songs, please consider buying the full game.",
                        "Demo Mode", autoTimeOutTime: 5f, button1Txt: "Okay");
            NotificationManager.RequestNotification(visuals);
        }
        else
        {
            DownloadSong();
        }
    }

    public void DownloadSong()
    {
        if (_activeBeatmap == null)
        {
            return;
        }

        DownloadSongAsync().Forget();
    }

    public void RequestMoveForward(float scrollValue)
    {
        MoveForwardAsync(scrollValue).Forget();
    }

    public void RequestMoveBackward(float scrollValue)
    {
        MoveBackwardAsync(scrollValue).Forget();
    }

    private async UniTaskVoid RequestLatestAsync()
    {
        await RefreshToken();

        if (_beatSaver == null)
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancellationTokenSource.Token);
        }
        var request = await _beatSaver.LatestBeatmaps(token: _cancellationTokenSource.Token);
        if (request == null)
        {
            return;
        }
        _activePage = request;
        _nextPage = await _activePage.Next(_cancellationTokenSource.Token);
        await UpdateDataForward();
    }

    private async UniTask RequestNextPageAsync(bool setData = true)
    {
        await RefreshToken();

        _showLoadingObject.SetActive(true);
        _activePage = _nextPage;
        _nextPage = await _activePage.Next(_cancellationTokenSource.Token);
        if (_activePage == null || !setData)
        {
            return;
        }

        await UpdateDataForward();
    }

    private async UniTask RequestPreviousPageAsync(bool setData = true)
    {
        await RefreshToken();
        _showLoadingObject.SetActive(true);
        _nextPage = _activePage;
        _activePage = await _activePage.Previous(_cancellationTokenSource.Token);

        if (_activePage == null || !setData)
        {
            return;
        }
        await UpdateDataBackwards();
    }

    private async UniTask SearchAsync(SearchTextFilterOption option)
    {
        try
        {
            await RefreshToken();
            if (_beatSaver == null)
            {
                await UniTask.DelayFrame(1, cancellationToken: _cancellationTokenSource.Token);
            }
            var request = await _beatSaver.SearchBeatmaps(option, token: _cancellationTokenSource.Token);
            if (request == null)
            {
                return;
            }
            _activePage = request;
            _nextPage = await _activePage.Next(token: _cancellationTokenSource.Token);
        }
        catch (Exception e)
        {
            await UniTask.SwitchToMainThread(_cancellationTokenSource.Token);

            if (e is not System.Threading.Tasks.TaskCanceledException)
            {
                Debug.LogError(e);
            }
            _showLoadingObject.SetActive(false);
            return;
        }

        await UpdateDataForward();
    }

    private async UniTaskVoid PlaySongAudioAsync()
    {
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
            return;
        }

        await RefreshToken();

        var audioClip = await _activeBeatmap.LatestVersion.GetPlayablePreview(_cancellationTokenSource.Token);
        if (audioClip == null)
        {
            NotificationManager.RequestNotification(new Notification.NotificationVisuals("Preview failed."));
            Debug.LogError("Preview Failed");
            return;
        }

        await UniTask.DelayFrame(1);
        _audioSource.clip = audioClip;
        _audioSource.Play();
        await UniTask.WaitUntil(() => !_audioSource.isPlaying && FocusTracker.Instance.IsFocused);
    }

    private async UniTaskVoid DownloadSongAsync()
    {
        var activeCell = _cellView;
        var folderName =
            $"{_activeBeatmap.ID} ({_activeBeatmap.Metadata.SongName} - {_activeBeatmap.Metadata.LevelAuthorName})";

        _downloadButton.interactable = false;
        var shouldContinue = await VerifyShouldDownload(folderName);
        if (!shouldContinue)
        {
            _downloadButton.interactable = true;
            Debug.Log("Will Not Download");
            return;
        }

        var beatmapID = _activeBeatmap.ID;
        _downloadingIds.Add(beatmapID);
        var progress = new Progress<double>();
        var loadingDisplay = _loadingDisplays.DisplayNewLoading(_activeBeatmap.Metadata.SongName);
        if (loadingDisplay != null)
        {
            progress.ProgressChanged += (sender, d) => loadingDisplay.UpdateLoadingBar(d);
        }

        if (_downloadsTokenSource == null || _downloadsTokenSource.IsCancellationRequested)
        {
            await RefreshDownloadsToken();
        }

        var songBytes = await _activeBeatmap.LatestVersion.DownloadZIP(_downloadsTokenSource.Token, progress);
        if (songBytes == null || _downloadsTokenSource.IsCancellationRequested)
        {
            NotificationManager.RequestNotification(new Notification.NotificationVisuals("Download failed."));
            Debug.LogError("Download Failed");
            return;
        }

        await UniTask.DelayFrame(1);
        ZipFileManagement.ExtractAndSaveZippedSongAsync(folderName, songBytes);
        await UniTask.DelayFrame(1);
        await UniTask.SwitchToMainThread(_downloadsTokenSource.Token);
        _downloadingIds.Remove(beatmapID);
        if (beatmapID == _activeBeatmap.ID)
        {
            _downloadButton.interactable = true;
        }
        activeCell.SetDownloaded(true);
        await SongInfoFilesReader.Instance.LoadNewSong(folderName);

        PlaylistFilesReader.Instance.RefreshPlaylistsValidStates().Forget();
    }

    #endregion

    private async UniTask UpdateData()
    {
        await UniTask.SwitchToMainThread(_cancellationTokenSource.Token);

        _cellView = null;

        _showLoadingObject.SetActive(false);
        _scrollerController.SetBeatmaps(_activePage.Beatmaps);
    }

    private async UniTask UpdateDataForward()
    {
        await UniTask.SwitchToMainThread(_cancellationTokenSource.Token);
        if (_token.IsCancellationRequested)
        {
            return;
        }
        _cellView = null;

        _showLoadingObject.SetActive(false);
        _allBeatmaps.Clear();
        _allBeatmaps.AddRange(_activePage.Beatmaps);
        if (_nextPage != null)
        {
            _allBeatmaps.AddRange(_nextPage.Beatmaps);
        }

        _scrollerController.SetBeatmaps(_allBeatmaps, resetPageIndex: true);
    }

    private async UniTask UpdateDataBackwards()
    {
        await UniTask.SwitchToMainThread(_cancellationTokenSource.Token);

        _cellView = null;

        _showLoadingObject.SetActive(false);
        _allBeatmaps.Clear();
        _allBeatmaps.AddRange(_activePage.Beatmaps);
        if (_nextPage != null)
        {
            _allBeatmaps.AddRange(_nextPage.Beatmaps);
        }

        _scrollerController.SetBeatmaps(_allBeatmaps);
    }

    private async UniTaskVoid MoveForwardAsync(float scrollValue)
    {
        _allBeatmaps.Clear();
        _scrollerController.Disable();
        await RequestNextPageAsync(false);
        await FinishMovePage(scrollValue);
    }

    private async UniTaskVoid MoveBackwardAsync(float scrollValue)
    {
        _allBeatmaps.Clear();
        _scrollerController.Disable();
        await RequestPreviousPageAsync(false);
        await FinishMovePage(scrollValue);
    }

    private async UniTask FinishMovePage(float scrollValue)
    {
        if (_activePage != null)
        {
            _allBeatmaps.AddRange(_activePage.Beatmaps);
        }
        if (_nextPage != null)
        {
            _allBeatmaps.AddRange(_nextPage.Beatmaps);
        }
        await UniTask.SwitchToMainThread(_cancellationTokenSource.Token);
        _showLoadingObject.SetActive(false);
        _scrollerController.Enable();
        _scrollerController.SetBeatmaps(_allBeatmaps, scrollValue);
    }

    public void SetActiveBeatmap(Beatmap beatmap)
    {
        _activeBeatmap = beatmap;
        UpdateUI().Forget();
    }

    public void SetSelectedCellView(BeatSaverSongCellView cellView)
    {
        _cellView = cellView;
    }

    private async UniTaskVoid UpdateUI()
    {
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
        if (_activeBeatmap == null)
        {
            _infoCardHolder.SetActive(false);
            return;
        }

        _infoCardHolder.SetActive(true);

        var minutes = _activeBeatmap.Metadata.Duration / MINUTE;
        var seconds = _activeBeatmap.Metadata.Duration % MINUTE;

        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.AppendFormat(SONGINFOFORMAT,
                _activeBeatmap.Metadata.SongName,
                _activeBeatmap.Metadata.SongAuthorName,
                _activeBeatmap.Metadata.LevelAuthorName,
                minutes,
                seconds,
                _activeBeatmap.Stats.Score * 100,
                _activeBeatmap.Uploaded.ToShortDateString());

            _songDetails.SetText(sb);
        }

        var difficultyNames = new string[_activeBeatmap.LatestVersion.Difficulties.Count];
        for (var i = 0; i < difficultyNames.Length; i++)
        {
            var difficulty = _activeBeatmap.LatestVersion.Difficulties[i].Difficulty.ToString().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(difficulty))
            {
                difficulty = char.ToUpper(difficulty[0]) + difficulty[1..];
            }

            difficultyNames[i] = difficulty;
        }

        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.AppendJoin(", ", difficultyNames);
            _mapDifficulties.SetText(sb);
        }

        var imageBytes = await _activeBeatmap.LatestVersion.DownloadCoverImage(token: _cancellationTokenSource.Token);
        if (imageBytes != null)
        {
            await UniTask.SwitchToMainThread(_cancellationTokenSource.Token);
            _activeBeatmapImage = new Texture2D(1, 1);
            _activeBeatmapImage.LoadImage(imageBytes);
            _songImage.sprite = Sprite.Create(_activeBeatmapImage,
                new Rect(0, 0, _activeBeatmapImage.width, _activeBeatmapImage.height), Vector2.one * .5f, 100f);
        }

        _downloadButton.interactable = !_downloadingIds.Contains(_activeBeatmap.ID);
        WaitAndPlayPreview().Forget();
    }

    private async UniTaskVoid WaitAndPlayPreview()
    {
        var activeMap = _activeBeatmap;
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        if (activeMap != _activeBeatmap)
        {
            return;
        }
        PlaySongAudioAsync().Forget();
    }

    private async UniTask<bool> VerifyShouldDownload(string folderName)
    {
        var shouldDownload = true;
        await UniTask.SwitchToMainThread(_cancellationTokenSource.Token);

        if (_levelFileManagement.FolderExists(folderName))
        {
            var visuals = new Notification.NotificationVisuals(
                $"The song {_activeBeatmap.Metadata.SongName} already exists on your device. Would you like to download and replace the song?",
                "Song Already Exists", "Yes", "No");
            var notification = NotificationManager.RequestNotification(visuals, () => shouldDownload = true,
                () => shouldDownload = false);
            await UniTask.WaitUntil(() => notification.IsPooled, cancellationToken: _cancellationTokenSource.Token);

            if (shouldDownload)
            {
                _levelFileManagement.DeleteFolder(folderName);
                await UniTask.DelayFrame(1, cancellationToken: _cancellationTokenSource.Token);
            }
        }

        return shouldDownload;
    }

    private async UniTask RefreshToken()
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            await UniTask.DelayFrame(1);
            _cancellationTokenSource.Dispose();
        }

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_token);
    }

    private async UniTask RefreshDownloadsToken()
    {
        {
            if (_downloadsTokenSource != null)
            {
                _downloadsTokenSource.Cancel();
                await UniTask.DelayFrame(1);
                _downloadsTokenSource.Dispose();
                CancelDownloads();
            }

            _downloadsTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_token);
        }
    }

    private void CancelDownloads()
    {
        _loadingDisplays.CancelAll();
        _downloadingIds.Clear();
        _downloadButton.interactable = true;
    }
}