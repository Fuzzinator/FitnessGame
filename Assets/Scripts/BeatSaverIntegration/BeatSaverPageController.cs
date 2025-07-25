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
using EnhancedUI.EnhancedScroller;
using System.IO;

public class BeatSaverPageController : MonoBehaviour
{
    [SerializeField]
    private BeatSaverSongsScrollerController _scrollerController;

    [SerializeField]
    private LoadingProgressUIDisplay _loadingProgressObj;
    [SerializeField]
    private CanvasGroup _canvasController;

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
    private Button _playButton;
    [SerializeField]
    private Button _deleteButton;

    [SerializeField]
    private LoadingDisplaysController _loadingDisplays;

    [SerializeField]
    private CanvasGroup _playSongsCanvas;
    [SerializeField]
    private SetAndShowSongOptions _showSongOptions;

    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private EndlessEnhancedScroller _scroller;

    [SerializeField]
    private TMP_Dropdown_XRSupport _sortDropdown;

    private Page _activePage;
    private Page _nextPage;

    private CancellationToken _token;
    private CancellationTokenSource _cancellationTokenSource;
    private CancellationTokenSource _downloadsTokenSource;
    private BeatSaver _beatSaver;
    private LevelFileManagement _levelFileManagement;
    private SearchTextFilterOption _currentSortOption;

    private Beatmap _activeBeatmap;

    private Texture2D _activeBeatmapImage;

    private List<string> _downloadingIds = new List<string>();

    private BeatSaverSongCellView _cellView;

    private List<Beatmap> _allBeatmaps = new List<Beatmap>();

    private const int MINUTE = 60;

    private const int RatingSortMode = 0;
    private const int RelevanceSortMode = 1;
    private const int LatestSortMode = 2;
    private const int CuratedSortMode = 3;

    private const string SONGINFOFORMAT =
        "<voffset=10>Song Name:           {0}\nSong Author:<line-indent=1>         {1}<line-indent=0>\nLevel Author:<line-indent=1>         {2}<line-indent=0>\nSong Length:         <line-indent=1>{3}:{4}<line-indent=0>\nSong Score:          <line-indent=1> {5}<line-indent=0>\nSong BPM:             <line-indent=0>{6}<line-indent=0>\nUpload Date:         <line-indent=2>{7}<line-indent=0>";

    public void Initialize()
    {
        _token = this.GetCancellationTokenOnDestroy();
        _beatSaver = new BeatSaver(Application.productName, Version.Parse(Application.version));
        _scrollerController.SetPageController(this);

        ZipFileManagement.Initialize(AssetManager.SongsPath);
        _levelFileManagement = new LevelFileManagement(AssetManager.SongsPath);
    }

    private void OnEnable()
    {
        _activeBeatmap = null;
        UpdateUI().Forget();
    }

    private void OnDisable()
    {
        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
        if (_playSongsCanvas != null)
        {
            _playSongsCanvas.gameObject.SetActive(false);
        }
        Reset();
    }

    private void Reset()
    {
        RefreshToken().Forget();
        ShowLoading(false);
        _scroller.SetCanScroll(false);
    }

    public void RequestFilterBy(int sortingOptions)
    {
        ShowLoading(true);
        _currentSortOption = ((SortingOptions)sortingOptions) switch
        {
            SortingOptions.Latest => SearchTextFilterOption.Latest,
            SortingOptions.Relevance => SearchTextFilterOption.Relevance,
            SortingOptions.Rating => SearchTextFilterOption.Rating,
            SortingOptions.Curated => SearchTextFilterOption.Curated,
            _ => throw new ArgumentOutOfRangeException(nameof(sortingOptions), sortingOptions, null)
        };
        _currentSortOption.Query = _inputField.text;

        SearchAsync(_currentSortOption).Forget();
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
        ShowLoading(true);
        _currentSortOption = SearchTextFilterOption.Curated;
        _currentSortOption.Query = _inputField.text;

        SearchAsync(_currentSortOption).Forget();
        _sortDropdown.SetValueWithoutNotify((int)SortingOptions.Curated);
    }

    public void RequestLatest()
    {
        ShowLoading(true);
        _currentSortOption = SearchTextFilterOption.Latest;
        _currentSortOption.Query = _inputField.text;
        SearchAsync(_currentSortOption, true).Forget();
        _sortDropdown.SetValueWithoutNotify((int)SortingOptions.Latest);
        //RequestLatestAsync().Forget();
    }

    public void InitializeServerConnection()
    {
        ShowLoading(true);
        _currentSortOption = SearchTextFilterOption.Rating;
        _currentSortOption.Query = _inputField.text;

        SearchAsync(_currentSortOption, true).Forget();
        _sortDropdown.SetValueWithoutNotify((int)SortingOptions.Rating);
    }

    public void RequestHighestRated()
    {
        _sortDropdown.SetValueWithoutNotify(RatingSortMode);
        ShowLoading(true);
        _currentSortOption = SearchTextFilterOption.Rating;
        _currentSortOption.Query = _inputField.text;

        SearchAsync(_currentSortOption).Forget();
        _sortDropdown.SetValueWithoutNotify((int)SortingOptions.Rating);
    }

    public void RequestMostRelevant()
    {
        ShowLoading(true);
        _currentSortOption = SearchTextFilterOption.Relevance;
        _currentSortOption.Query = _inputField.text;
        SearchAsync(_currentSortOption).Forget();
        _sortDropdown.SetValueWithoutNotify((int)SortingOptions.Relevance);
    }

    public void RequestNextPage()
    {
        if (_activePage == null)
        {
            return;
        }

        ShowLoading(true);
        RequestNextPageAsync().Forget();
    }

    public void RequestPreviousPage()
    {
        if (_activePage == null)
        {
            return;
        }

        ShowLoading(true);
        RequestPreviousPageAsync().Forget();
    }

    public void RequestSearch(TMP_InputField textField)
    {
        ShowLoading(true);
        Search(textField.text);
    }

    public void Search(string search)
    {
        if (_activePage == null)
        {
            return;
        }

        ShowLoading(true);
        _currentSortOption.Query = search;
        SearchAsync(_currentSortOption).Forget();
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
        if (GameManager.Instance.DemoMode && SongInfoFilesReader.Instance.CustomSongsCount >= GameManager.DemoModeMaxCustomSongs)
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

    private async UniTask RequestLatestAsync()
    {
        await RefreshToken();

        ShowLoading(true);
        _scroller.SetCanScroll(false);
        if (_beatSaver == null)
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancellationTokenSource.Token).SuppressCancellationThrow();
        }

        Page request = null;
        try
        {
            request = await _beatSaver.LatestBeatmaps(token: _cancellationTokenSource.Token, progress: _loadingProgressObj.ProgressDisplay);
        }
        catch (Exception ex)
        {
            ShowLoading(false);

            if (ex is TimeoutException)
            {
                ErrorReporter.SetSuppressed(true);
                Debug.LogError($"Failed to retrieve latest beatmaps. Error:{ex.Message}--{ex.StackTrace}");
                ErrorReporter.SetSuppressed(false);
                var visuals = new Notification.NotificationVisuals($"Failed to retrieve latest songs because the connection timed out. Would you like to try again?", "Connection Timed Out", "Yes", "No");
                NotificationManager.RequestNotification(visuals, () =>
                {
                    RequestLatestAsync().Forget();
                });
            }
            else
            {
                Debug.LogError($"Failed to retrieve latest beatmaps. Error:{ex.Message}--{ex.StackTrace}");
            }
        }

        if (request == null)
        {
            return;
        }
        _activePage = request;
        _nextPage = await _activePage.Next(_cancellationTokenSource.Token, _loadingProgressObj.ProgressDisplay);
        await UpdateDataForward();
    }

    private async UniTask RequestNextPageAsync(bool setData = true)
    {
        await RefreshToken();

        ShowLoading(true);
        _scroller.SetCanScroll(false);
        var previousPage = _activePage;
        _activePage = _nextPage ?? previousPage;

        if (_cancellationTokenSource == null)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_token);
        }
        if (_activePage == null)
        {
            await RequestLatestAsync();
            return;
        }
        _nextPage = await _activePage.Next(_cancellationTokenSource.Token, _loadingProgressObj.ProgressDisplay);

        if (_activePage == null || !setData)
        {
            return;
        }

        await UpdateDataForward();
    }

    private async UniTask RequestPreviousPageAsync(bool setData = true)
    {
        await RefreshToken();
        ShowLoading(true);
        _scroller.SetCanScroll(false);
        _nextPage = _activePage;

        if (_cancellationTokenSource == null)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_token);
        }
        if (_activePage == null)
        {
            await RequestLatestAsync();
            return;
        }
        _activePage = await _activePage.Previous(_cancellationTokenSource.Token, _loadingProgressObj.ProgressDisplay);

        if (_activePage == null || !setData)
        {
            return;
        }
        await UpdateDataBackwards();
    }

    private async UniTask SearchAsync(SearchTextFilterOption option, bool initializing = false) 
    {
        try
        {
            await RefreshToken();
            if (_beatSaver == null)
            {
                await UniTask.DelayFrame(1, cancellationToken: _cancellationTokenSource.Token).SuppressCancellationThrow();
            }
            _scroller.SetCanScroll(false);

            Page request = null;
            try
            {
                request = await _beatSaver.SearchBeatmaps(option, token: _cancellationTokenSource.Token, progress: _loadingProgressObj.ProgressDisplay);
            }
            catch (Exception ex)
            {
                ShowLoading(false);

                if (ex is TimeoutException)
                {
                    ErrorReporter.SetSuppressed(true);
                    Debug.LogError($"Failed to search beatmaps due to connection timeout. Attempted search:{option.Query}-{option.SortOrder}. Error:{ex.Message}--{ex.StackTrace}");
                    ErrorReporter.SetSuppressed(false);

                    if (!initializing)
                    {
                        var visuals = new Notification.NotificationVisuals($"Failed to retrieve songs because the connection timed out. Would you like to try again?", "Connection Timed Out", "Yes", "No");
                        NotificationManager.RequestNotification(visuals, () =>
                        {
                            SearchAsync(option).Forget();
                        });
                    }
                }
                else
                {
                    Debug.LogError($"Failed to search beatmaps. Attempted search:{option.Query}-{option.SortOrder}. Error:{ex.Message}--{ex.StackTrace}");
                }
            }
            if (request == null)
            {
                await ClearPage();
                return;
            }
            _activePage = request;
            _nextPage = await _activePage.Next(token: _cancellationTokenSource.Token, _loadingProgressObj.ProgressDisplay);
        }
        catch (Exception e)
        {
            if (e is Newtonsoft.Json.JsonSerializationException)
            {
                return;
            }
            await UniTask.SwitchToMainThread(_cancellationTokenSource.Token);

            if (e is not System.Threading.Tasks.TaskCanceledException)
            {
                Debug.LogError(e);
            }
            ShowLoading(false);
            _scroller.SetCanScroll(true);
            return;
        }

        await UpdateDataForward();
    }

    private async UniTaskVoid PlaySongAudioAsync()
    {
        if (_audioSource == null)
        {
            return;
        }
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
            return;
        }
        var targetBeatmap = _activeBeatmap;
        await RefreshToken();
        if (_activeBeatmap != targetBeatmap || targetBeatmap?.LatestVersion == null || _audioSource == null)
        {
            return;
        }


        if (_cancellationTokenSource == null)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_token);
        }
        AudioClip audioClip = null;
        try
        {
            audioClip = await targetBeatmap.LatestVersion.GetPlayablePreview(_cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {

        }

        await UniTask.DelayFrame(1);
        if (_audioSource == null)
        {
            return;
        }
        if(audioClip == null)
        {
            return;
        }
        _audioSource.clip = audioClip;
        _audioSource.Play();
        await UniTask.WaitUntil(() => _audioSource == null || !_audioSource.isPlaying && FocusTracker.Instance.IsFocused);
    }

    private async UniTaskVoid DownloadSongAsync()
    {
        var activeCell = _cellView;
        var targetBeatmap = _activeBeatmap;
        var folderName = GetFolderName();

        _downloadButton.interactable = false;
        var shouldContinue = await VerifyShouldDownload(folderName);
        if (!shouldContinue)
        {
            _downloadingIds?.Remove(targetBeatmap.ID);
            activeCell?.SetDownloaded(false);
            if (targetBeatmap.ID == _activeBeatmap.ID)
            {
                _downloadButton.interactable = true;
            }
            Debug.Log("Will Not Download");
            return;
        }
        _downloadingIds.Add(targetBeatmap.ID);
        var progress = new Progress<double>();
        if(_activeBeatmap == null)
        {
            Debug.LogError("Attempting to download a song but _activeBeatmap is null.");
            _downloadingIds?.Remove(targetBeatmap.ID);
            activeCell?.SetDownloaded(false);
            if (targetBeatmap.ID == _activeBeatmap.ID)
            {
                _downloadButton.interactable = true;
            }
            return;
        }
        var songName = _activeBeatmap.Metadata?.SongName ?? _activeBeatmap.Name;
        var loadingDisplay = _loadingDisplays.DisplayNewLoading(songName);
        if (loadingDisplay != null)
        {
            progress.ProgressChanged += (sender, d) =>
            {
                if (loadingDisplay == null)
                {
                    return;
                }
                loadingDisplay.UpdateLoadingBar(d);
            };
        }

        if (_downloadsTokenSource == null || _downloadsTokenSource.IsCancellationRequested)
        {
            await RefreshDownloadsToken();
        }
        byte[] songBytes = await DownloadZip(progress, targetBeatmap, loadingDisplay, activeCell);

        if (songBytes == null || _downloadsTokenSource.IsCancellationRequested)
        {
            var visuals = new Notification.NotificationVisuals($"Failed to download {targetBeatmap.Name}", "Download failed.", autoTimeOutTime: 1f, popUp: true);
            NotificationManager.RequestNotification(visuals);
            Debug.LogError("Download Failed");

            _downloadingIds?.Remove(targetBeatmap.ID);
            activeCell?.SetDownloaded(false);
            if (targetBeatmap.ID == _activeBeatmap.ID)
            {
                _downloadButton.interactable = true;
            }
            return;
        }

        await UniTask.DelayFrame(1);
        if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
        {
            _downloadingIds?.Remove(targetBeatmap.ID);
            activeCell?.SetDownloaded(false);
            if (targetBeatmap.ID == _activeBeatmap.ID)
            {
                _downloadButton.interactable = true;
            }
            return;
        }
        try
        {
            if (Directory.Exists(folderName))
            {
                Directory.Delete(folderName, true);
            }
            ZipFileManagement.ExtractAndSaveZippedSong(folderName, songBytes);
        }
        catch (Exception ex)
        {
            Debug.LogError($"{folderName} cant be saved might have illegal characters {ex.Message} -- {ex.StackTrace}");
        }
        await UniTask.DelayFrame(1);
        await UniTask.SwitchToMainThread(_downloadsTokenSource.Token);
        _downloadingIds?.Remove(targetBeatmap.ID);
        if (targetBeatmap.ID == _activeBeatmap.ID)
        {
            _downloadButton.interactable = true;
        }
        activeCell?.SetDownloaded(true);

        //TODO: Need to remove the existing song if a duplicate exists in the SongInfoFilesReader
        var score = 0f;
        if (targetBeatmap.Stats != null)
        {
            score = targetBeatmap.Stats.Score;
        }
        await SongInfoFilesReader.Instance.LoadNewSong(folderName, targetBeatmap.ID, score, false);

        PlaylistFilesReader.Instance.RefreshPlaylistsValidStates().Forget();
        UpdateUI().Forget();
    }

    /*private async UniTask DownloadBeatmapAsync(Beatmap targetBeatmap)
    {
        _downloadingIds.Add(targetBeatmap.ID);
        var progress = new Progress<double>();
        var songName = _activeBeatmap.Metadata?.SongName ?? _activeBeatmap.Name;
        var loadingDisplay = _loadingDisplays.DisplayNewLoading(songName);
        if (loadingDisplay != null)
        {
            progress.ProgressChanged += (sender, d) => loadingDisplay.UpdateLoadingBar(d);
        }

        if (_downloadsTokenSource == null || _downloadsTokenSource.IsCancellationRequested)
        {
            await RefreshDownloadsToken();
        }
        byte[] songBytes = await DownloadZip(progress, targetBeatmap, loadingDisplay, activeCell);

        if (songBytes == null || _downloadsTokenSource.IsCancellationRequested)
        {
            var visuals = new Notification.NotificationVisuals($"Failed to download {targetBeatmap.Name}", "Download failed.", autoTimeOutTime: 1f, popUp: true);
            NotificationManager.RequestNotification(visuals);
            Debug.LogError("Download Failed");
            return;
        }

        await UniTask.DelayFrame(1);
        if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }
        try
        {
            if (Directory.Exists(folderName))
            {
                Directory.Delete(folderName, true);
            }
            ZipFileManagement.ExtractAndSaveZippedSong(folderName, songBytes);
        }
        catch (Exception ex)
        {
            Debug.LogError($"{folderName} cant be saved might have illegal characters {ex.Message} -- {ex.StackTrace}");
        }
        await UniTask.DelayFrame(1);
        await UniTask.SwitchToMainThread(_downloadsTokenSource.Token);
        _downloadingIds.Remove(targetBeatmap.ID);
        if (targetBeatmap.ID == _activeBeatmap.ID)
        {
            _downloadButton.interactable = true;
        }

        //TODO: Need to remove the existing song if a duplicate exists in the SongInfoFilesReader
        var score = 0f;
        if (targetBeatmap.Stats != null)
        {
            score = targetBeatmap.Stats.Score;
        }
        await SongInfoFilesReader.Instance.LoadNewSong(folderName, targetBeatmap.ID, score);

        PlaylistFilesReader.Instance.RefreshPlaylistsValidStates().Forget();
        UpdateUI().Forget();
    }*/

    private async UniTask<byte[]> DownloadZip(Progress<double> progress, Beatmap targetBeatmap, LoadingDisplay loadingDisplay, BeatSaverSongCellView activeCell)
    {
        try
        {
            if (_downloadsTokenSource == null)
            {
                _downloadsTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_token);
            }
            var songBytes = await _activeBeatmap.LatestVersion.DownloadZIP(_downloadsTokenSource.Token, progress);
            return songBytes;
        }
        catch (Exception ex)
        {
            if (ex is TimeoutException)
            {
                if (targetBeatmap == null)
                {
                    return null;
                }

                _downloadingIds.Remove(targetBeatmap.ID);
                loadingDisplay.ReturnToPool();

                var visuals = new Notification.NotificationVisuals($"{targetBeatmap.Name} failed to download because the connection timed out. Would you like to try again?", "Download Failed", "Yes", "No");
                NotificationManager.RequestNotification(visuals, () =>
                {
                    _cellView = activeCell;
                    _activeBeatmap = targetBeatmap;
                    DownloadSongAsync().Forget();
                });
                return null;
            }
            else
            {
                Debug.LogError(ex);
            }
        }
        return null;
    }

    #endregion

    private async UniTask UpdateData()
    {
        await UniTask.SwitchToMainThread(_cancellationTokenSource.Token);

        _cellView = null;

        ShowLoading(false);
        _scrollerController.SetBeatmaps(_activePage.Beatmaps).Forget();
    }

    private async UniTask ClearPage()
    {

        await UniTask.SwitchToMainThread(_cancellationTokenSource.Token);
        if (_token.IsCancellationRequested)
        {
            return;
        }
        _cellView = null;

        ShowLoading(false);
        _allBeatmaps.Clear();

        _scrollerController.SetBeatmaps(_allBeatmaps, resetPageIndex: true).Forget();
        _scrollerController.Refresh();
        _scroller.SetCanScroll(false);
    }

    private async UniTask UpdateDataForward()
    {
        await UniTask.SwitchToMainThread(_cancellationTokenSource.Token);
        if (_token.IsCancellationRequested)
        {
            return;
        }
        _cellView = null;

        ShowLoading(false);
        _allBeatmaps.Clear();
        _allBeatmaps.AddRange(_activePage.Beatmaps);
        if (_nextPage != null)
        {
            _allBeatmaps.AddRange(_nextPage.Beatmaps);
        }

        _scrollerController.SetBeatmaps(_allBeatmaps, resetPageIndex: true).Forget();
        _scroller.SetCanScroll(true);
    }

    private async UniTask UpdateDataBackwards()
    {
        await UniTask.SwitchToMainThread(_cancellationTokenSource.Token);

        _cellView = null;

        ShowLoading(false);
        _allBeatmaps.Clear();
        _allBeatmaps.AddRange(_activePage.Beatmaps);
        if (_nextPage != null)
        {
            _allBeatmaps.AddRange(_nextPage.Beatmaps);
        }

        _scrollerController.SetBeatmaps(_allBeatmaps).Forget();
        _scroller.SetCanScroll(true);
    }

    private async UniTaskVoid MoveForwardAsync(float scrollValue)
    {
        _allBeatmaps.Clear();
        _scrollerController.Disable();
        _scroller.SetCanScroll(false);
        await RequestNextPageAsync(false);
        await FinishMovePage(scrollValue);
    }

    private async UniTaskVoid MoveBackwardAsync(float scrollValue)
    {
        _allBeatmaps.Clear();
        _scrollerController.Disable();
        _scroller.SetCanScroll(false);
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
        ShowLoading(false);
        _scrollerController.Enable();
        _scrollerController.SetBeatmaps(_allBeatmaps, scrollValue).Forget();
        _scroller.SetCanScroll(true);
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
        var songDownloaded = _levelFileManagement.FolderExists(GetFolderName());
        _playButton.interactable = songDownloaded;
        _deleteButton.interactable = songDownloaded;
        _playSongsCanvas.gameObject.SetActive(false);


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
                _activeBeatmap.Metadata.BPM,
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
        byte[] imageBytes = null;
        try
        {
            imageBytes = await _activeBeatmap.LatestVersion.DownloadCoverImage(token: _cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            if (ex is TimeoutException)
            {
                ErrorReporter.SetSuppressed(true);
                Debug.LogError($"Connection timed out to download image for {_activeBeatmap.Name}-{_activeBeatmap.ID}. Error:{ex.Message}--{ex.StackTrace}");
                ErrorReporter.SetSuppressed(false);
            }
            else
            {
                Debug.LogError($"Failed to load image for song {_activeBeatmap.Name}-{_activeBeatmap.ID}. Error:{ex.Message}--{ex.StackTrace}");
            }
        }
        if (imageBytes != null)
        {
            await UniTask.SwitchToMainThread(_cancellationTokenSource.Token);
            _activeBeatmapImage = new Texture2D(1, 1);
            _activeBeatmapImage.LoadImage(imageBytes);
            _songImage.sprite = Sprite.Create(_activeBeatmapImage,
                new Rect(0, 0, _activeBeatmapImage.width, _activeBeatmapImage.height), Vector2.one * .5f, 100f, 0, SpriteMeshType.FullRect);
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
            var notification = await NotificationManager.RequestNotificationAsync(visuals, () => shouldDownload = true,
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
            await UniTask.DelayFrame(1).SuppressCancellationThrow();
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

    private void ShowLoading(bool isLoading)
    {
        _loadingProgressObj.gameObject.SetActive(isLoading);
        _canvasController.interactable = !isLoading;
    }

    public void DeleteSong()
    {
        var beatmap = _activeBeatmap;
        _deleteButton.interactable = false;
        _playButton.interactable = false;
        AssetManager.DeleteCustomSong(GetFolderName());
    }

    public void StartPlaySong()
    {
        var songInfo = SongInfoFilesReader.Instance.TryGetSongInfo(GetFolderName());
        if (songInfo == null)
        {
            var visuals = new Notification.NotificationVisuals("Song could not be found in available songs.", "Cannot Load Song", "Okay");
            NotificationManager.RequestNotification(visuals);
            return;
        }

        _playSongsCanvas.gameObject.SetActive(true);

        _showSongOptions.UpdateDifficultyOptions(songInfo, songInfo.DifficultySets);
    }

    public void CancelPlaySong()
    {
        _playSongsCanvas.gameObject.SetActive(false);
    }

    private string GetFolderName()
    {
        return $"{_activeBeatmap.ID} ({_activeBeatmap.Metadata.SongName} - {_activeBeatmap.Metadata.LevelAuthorName})".RemoveIllegalIOCharacters();
    }
}