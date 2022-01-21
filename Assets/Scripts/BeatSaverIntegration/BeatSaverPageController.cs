using System;
using System.Collections;
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
using UnityEngine.PlayerLoop;

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


    private Page _activePage;

    private CancellationToken _cancellationToken;
    private BeatSaver _beatSaver;

    private Beatmap _activeBeatmap;

    private Texture2D _activeBeatmapImage;

    private const int MINUTE = 60;

    private void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
        _beatSaver = new BeatSaver(Application.productName, Version.Parse(Application.version));
        _scrollerController.SetPageController(this);
    }

    #region Webcalls

    public void RequestLatest()
    {
        RequestLatestAsync().Forget();
    }

    public void RequestNextPage()
    {
        if (_activePage == null)
        {
            return;
        }

        RequestNextPageAsync().Forget();
    }

    public void RequestPreviousPage()
    {
        if (_activePage == null)
        {
            return;
        }

        RequestPreviousPageAsync().Forget();
    }

    public void RequestSearch(TMP_InputField textField)
    {
        Search(textField.text);
    }
    
    public void Search(string search)
    {
        if (_activePage == null)
        {
            return;
        }

        var options = new SearchTextFilterOption(search);
        SearchAsync(options).Forget();
    }

    public void RequestHighestRated()
    {
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
        var request = await _beatSaver.SearchBeatmaps(searchOptions, token: _cancellationToken);
        if (request == null)
        {
            return;
        }

        _activePage = request;
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
        _showLoadingObject.SetActive(true);
        _activePage = await _activePage.Previous(_cancellationToken);
        if (_activePage == null)
        {
            return;
        }

        await UpdateData();
    }

    private async UniTaskVoid SearchAsync(SearchTextFilterOption option)
    {
        _showLoadingObject.SetActive(true);
        _activePage = await _beatSaver.SearchBeatmaps(option, token: _cancellationToken);
        if (_activePage == null)
        {
            return;
        }

        await UpdateData();
    }

    private async UniTaskVoid DownloadSongAsync()
    {
        var songBytes = await _activeBeatmap.LatestVersion.DownloadZIP(_cancellationToken);
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
    }
}