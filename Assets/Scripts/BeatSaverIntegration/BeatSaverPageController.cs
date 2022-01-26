using System;
using System.Collections.Generic;
using System.Threading;
using BeatSaverSharp;
using BeatSaverSharp.Models;
using BeatSaverSharp.Models.Pages;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UI.Scrollers.BeatsaverIntegraton;

public class BeatSaverPageController : MonoBehaviour
{
    [SerializeField]
    private BeatSaverSongsScrollerController _scrollerController;

    [SerializeField]
    private GameObject _showLoadingObject;

    [Header("Info Cards")]
    [SerializeField]
    private GameObject _infoCardHolder;

    [SerializeField]
    private TextMeshProUGUI _songName;

    [SerializeField]
    private TextMeshProUGUI _songAuthor;

    [SerializeField]
    private TextMeshProUGUI _levelAuthor;

    [SerializeField]
    private TextMeshProUGUI _levelLength;

    [SerializeField]
    private TextMeshProUGUI _songScore;

    [SerializeField]
    private TextMeshProUGUI _uploadDate;

    [SerializeField]
    private TextMeshProUGUI _mapDifficulties;

    [SerializeField]
    private Image _songImage;

    [SerializeField]
    private Button _downloadButton;

    [SerializeField]
    private LoadingDisplaysController _loadingDisplays;


    private Page _activePage;

    private CancellationToken _cancellationToken;
    private BeatSaver _beatSaver;
    private LevelFileManagement _levelFileManagement;

    private Beatmap _activeBeatmap;

    private Texture2D _activeBeatmapImage;

    private List<string> _downloadingIds = new List<string>();

    private const int MINUTE = 60;

    private void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
        _beatSaver = new BeatSaver(Application.productName, Version.Parse(Application.version));
        _scrollerController.SetPageController(this);

#if UNITY_ANDROID && !UNITY_EDITOR
        var directory = Application.persistentDataPath;
#elif UNITY_EDITOR
        //var dataPath = 
        var directory = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
#endif
        ZipFileManagement.Initialize(directory);
        _levelFileManagement = new LevelFileManagement(directory);
    }

    #region Webcalls

    public void RequestLatest()
    {
        _showLoadingObject.SetActive(true);
        RequestLatestAsync().Forget();
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

    public void RequestHighestRated()
    {
        _showLoadingObject.SetActive(true);
        RequestHighestRatedAsync().Forget();
    }

    public void DownloadSong()
    {
        if (_activeBeatmap == null)
        {
            return;
        }

        DownloadSongAsync().Forget();
    }

    private async UniTaskVoid RequestLatestAsync()
    {
        var request = await _beatSaver.LatestBeatmaps(token: _cancellationToken);
        if (request == null)
        {
            return;
        }

        _activePage = request;
        await UpdateData();
    }

    private async UniTaskVoid RequestHighestRatedAsync()
    {
        var searchOptions = new SearchTextFilterOption(string.Empty)
        {
            SortOrder = SortingOptions.Rating,
        };
        try
        {
            var request = await _beatSaver.SearchBeatmaps(searchOptions, token: _cancellationToken);
            if (request == null)
            {
                return;
            }

            _activePage = request;
        }
        catch (Exception e)
        {
            await UniTask.SwitchToMainThread(_cancellationToken);
            
            Debug.LogError(e);
            _showLoadingObject.SetActive(false);
            return;
        }

        await UpdateData();
    }

    private async UniTaskVoid RequestNextPageAsync()
    {
        _showLoadingObject.SetActive(true);
        _activePage = await _activePage.Next(_cancellationToken);
        if (_activePage == null)
        {
            return;
        }

        await UpdateData();
    }

    private async UniTaskVoid RequestPreviousPageAsync()
    {
        _activePage = await _activePage.Previous(_cancellationToken);
        if (_activePage == null)
        {
            return;
        }

        await UpdateData();
    }

    private async UniTaskVoid SearchAsync(SearchTextFilterOption option)
    {
        _activePage = await _beatSaver.SearchBeatmaps(option, token: _cancellationToken);
        if (_activePage == null)
        {
            return;
        }

        await UpdateData();
    }

    private async UniTaskVoid DownloadSongAsync()
    {
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

        var songBytes = await _activeBeatmap.LatestVersion.DownloadZIP(_cancellationToken, progress);
        if (songBytes == null)
        {
            NotificationManager.RequestNotification(new Notification.NotificationVisuals("Download failed."));
            Debug.LogError("Download Failed");
            return;
        }

        await UniTask.DelayFrame(1);
        ZipFileManagement.ExtractAndSaveZippedSongAsync(folderName, songBytes);
        await UniTask.DelayFrame(1);
        await UniTask.SwitchToMainThread(_cancellationToken);
        _downloadingIds.Remove(beatmapID);
        if (beatmapID == _activeBeatmap.ID)
        {
            _downloadButton.interactable = true;
        }

        SongInfoFilesReader.Instance.UpdateSongs().Forget();
    }

    #endregion

    private async UniTask UpdateData()
    {
        await UniTask.SwitchToMainThread(_cancellationToken);

        _showLoadingObject.SetActive(false);

        _scrollerController.SetBeatmaps(_activePage.Beatmaps);
    }

    public void SetActiveBeatmap(Beatmap beatmap)
    {
        _activeBeatmap = beatmap;
        UpdateUI().Forget();
    }

    private async UniTaskVoid UpdateUI()
    {
        if (_activeBeatmap == null)
        {
            _infoCardHolder.SetActive(false);
            return;
        }

        _infoCardHolder.SetActive(true);
        _songName.SetText(_activeBeatmap.Metadata.SongName);
        _songAuthor.SetText(_activeBeatmap.Metadata.SongAuthorName);
        _levelAuthor.SetText(_activeBeatmap.Metadata.LevelAuthorName);
        var minutes = _activeBeatmap.Metadata.Duration / MINUTE;
        var seconds = _activeBeatmap.Metadata.Duration % MINUTE;
        _levelLength.SetText($"{minutes}:{seconds}");
        _songScore.SetText($"{_activeBeatmap.Stats.Score * 100}");
        _uploadDate.SetText(_activeBeatmap.Uploaded.ToShortDateString());
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

        _mapDifficulties.SetText(string.Join(", ", difficultyNames));


        var imageBytes = await _activeBeatmap.LatestVersion.DownloadCoverImage(token: _cancellationToken);
        if (imageBytes != null)
        {
            await UniTask.SwitchToMainThread(_cancellationToken);
            _activeBeatmapImage = new Texture2D(1, 1);
            _activeBeatmapImage.LoadImage(imageBytes);
            _songImage.sprite = Sprite.Create(_activeBeatmapImage,
                new Rect(0, 0, _activeBeatmapImage.width, _activeBeatmapImage.height), Vector2.one * .5f, 100f);
        }

        _downloadButton.enabled = !_downloadingIds.Contains(_activeBeatmap.ID);
    }

    private async UniTask<bool> VerifyShouldDownload(string folderName)
    {
        var shouldDownload = true;
        await UniTask.SwitchToMainThread(_cancellationToken);

        if (_levelFileManagement.FolderExists(folderName))
        {
            var visuals = new Notification.NotificationVisuals(
                $"The song {_activeBeatmap.Metadata.SongName} already exists on your device. Would you like to download and replace the song?",
                "Song Already Exists", "Yes", "No");
            var notification = NotificationManager.RequestNotification(visuals, () => shouldDownload = true,
                () => shouldDownload = false);
            await UniTask.WaitUntil(() => notification.IsPooled, cancellationToken: _cancellationToken);

            if (shouldDownload)
            {
                _levelFileManagement.DeleteFolder(folderName);
                await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
            }
        }

        return shouldDownload;
    }
}