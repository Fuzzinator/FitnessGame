using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameModeManagement;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using static DifficultyInfo;
using static UnityEngine.XR.Hands.XRHandSubsystemDescriptor;

public class SetAndShowSongOptions : MonoBehaviour
{
    [SerializeField]
    private ToggleGroup _gameTypeToggleGroup;

    [SerializeField]
    private Toggle[] _gameTypeToggles;

    [SerializeField]
    private TextMeshProUGUI[] _gameTypeTexts;

    [SerializeField]
    private ToggleGroup _difficultyToggleGroup;
    [SerializeField]
    private Toggle[] _typeDifficultyToggles;

    [SerializeField]
    private TextMeshProUGUI[] _typeDifficultyTexts;

    [SerializeField]
    private DisplaySongRecords _songRecordsDisplay;

    [SerializeField]
    private SetTargetForwardFoot _forwardFootSetter;

    [SerializeField]
    private Button _playButton;
    [SerializeField]
    private TextMeshProUGUI _playButtonText;

    [SerializeField]
    private Button _previewButton;
    [SerializeField]
    private TextMeshProUGUI _previewButtonText;

    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private bool _autoPlayPreview = false;
    private bool _stopAutoPlay = false;

    public string SelectedDifficulty => _selectedDifficulty;
    public DifficultyInfo.DifficultyEnum DifficultyAsEnum => _difficultyEnum;
    public GameMode SelectedGameMode => _activeDifficultySet.MapGameMode;

    private SongInfo _songInfo;
    private SongInfo.DifficultySet[] _difficultySets;
    private SongInfo.DifficultySet _activeDifficultySet;
    private string _selectedDifficulty;
    private DifficultyInfo.DifficultyEnum _difficultyEnum;
    private CancellationToken _cancellationToken;
    private CancellationTokenSource _cancellationSource;
    private AsyncOperationHandle _currentSongRequestHandle = new AsyncOperationHandle();

    private bool _loadingSongPreview = false;

    public bool Initialized => _cancellationSource != null;

    #region Const Strings
    private const string Preview = "Listen";
    private const string Loading = "Loading";
    private const string Stop = "Stop";
    private const string DefaultGameModeSetting = "DefaultGameMode";
    private const string DefaultDifficultySetting = "DefaultDifficulty";
    #endregion

    private void Start()
    {
        Initialize();
    }

    private void OnDisable()
    {
        if (_autoPlayPreview && _stopAutoPlay)
        {
            StopSongPreview();
        }
    }

    public void Initialize()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
        _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
    }

    public void UpdateDifficultyOptions(SongInfo songInfo, SongInfo.DifficultySet[] difficultySets)
    {
        _songInfo = songInfo;
        _difficultySets = difficultySets;
        //gameObject.SetActive(true);
        _gameTypeToggleGroup.gameObject.SetActive(true);
        _difficultyToggleGroup.gameObject.SetActive(true);
        _playButton.gameObject.SetActive(true);
        _playButtonText.gameObject.SetActive(true);
        _previewButton?.gameObject.SetActive(true);
        _previewButtonText?.gameObject.SetActive(true);
        _gameTypeTexts[0].transform.parent.gameObject.SetActive(true);
        _typeDifficultyTexts[0].transform.parent.gameObject.SetActive(true);
        UpdateAvailableGameModes();

        if (_autoPlayPreview && gameObject.activeInHierarchy)
        {
            StopSongPreview();
            WaitAndPlayPreview().Forget();
        }
    }

    public void HideOptions()
    {
        //gameObject.SetActive(false);
        _gameTypeToggleGroup.gameObject.SetActive(false);
        _difficultyToggleGroup.gameObject.SetActive(false);
        _playButton.gameObject.SetActive(false);
        _playButtonText.gameObject.SetActive(false);
        _previewButton?.gameObject.SetActive(false);
        _previewButtonText?.gameObject.SetActive(false);
        _gameTypeTexts[0].transform.parent.gameObject.SetActive(false);
        _typeDifficultyTexts[0].transform.parent.gameObject.SetActive(false);

        if (_autoPlayPreview && gameObject.activeInHierarchy)
        {
            StopSongPreview();
        }
    }

    private void UpdateAvailableGameModes()
    {
        _stopAutoPlay = false;
        gameObject.SetActive(false);
        _gameTypeTexts[0].transform.parent.gameObject.SetActive(false);
        _typeDifficultyTexts[0].transform.parent.gameObject.SetActive(false);

        for (var i = 0; i < _gameTypeToggles.Length; i++)
        {
            var toggle = _gameTypeToggles[i];
            var text = _gameTypeTexts[i];

            toggle.gameObject.SetActive(false);
            text.gameObject.SetActive(false);
        }

        var lowest = 10;
        for (var i = 0; i < _difficultySets.Length; i++)
        {
            if (_difficultySets[i].MapGameMode is GameMode.LightShow or GameMode.Lawless)
            {
                continue;
            }
            var toggleID = (int)_difficultySets[i].MapGameMode - 1;

            /*if (_difficultySets[i].MapGameMode == GameMode.Lawless)
            {
                toggleID--;
            }*/

            if (toggleID < lowest)
            {
                lowest = toggleID;
            }
            _gameTypeToggles[toggleID].gameObject.SetActive(true);
            _gameTypeTexts[toggleID].gameObject.SetActive(true);
        }

        gameObject.SetActive(true);
        _gameTypeTexts[0].transform.parent.gameObject.SetActive(true);
        _typeDifficultyTexts[0].transform.parent.gameObject.SetActive(true);
        _stopAutoPlay = true;

        var defaultSetting = SettingsManager.GetSetting(DefaultGameModeSetting, lowest);

        _gameTypeToggles[defaultSetting].SetIsOnWithoutNotify(true);
        SetSelectedType(_gameTypeToggles[defaultSetting]);
    }

    private void UpdateAvailableDifficulties(bool setSelectedDifficulty)
    {
        _stopAutoPlay = false;
        gameObject.SetActive(false);

        _gameTypeTexts[0].transform.parent.gameObject.SetActive(false);
        _typeDifficultyTexts[0].transform.parent.gameObject.SetActive(false);

        for (var i = 0; i < _typeDifficultyToggles.Length; i++)
        {
            var toggle = _typeDifficultyToggles[i];
            var text = _typeDifficultyTexts[i];
            toggle.gameObject.SetActive(false);
            text.gameObject.SetActive(false);
        }

        var lowest = 10;
        for (var i = 0; i < _activeDifficultySet.DifficultyInfos.Length; i++)
        {
            var difficulty = (int)_activeDifficultySet.DifficultyInfos[i].DifficultyAsEnum - 1;
            if (difficulty < 0)
            {
                continue;
            }

            if (difficulty < lowest)
            {
                lowest = difficulty;
            }

            _typeDifficultyToggles[difficulty].gameObject.SetActive(true);
            _typeDifficultyTexts[difficulty].gameObject.SetActive(true);
        }

        gameObject.SetActive(true);
        _gameTypeTexts[0].transform.parent.gameObject.SetActive(true);
        _typeDifficultyTexts[0].transform.parent.gameObject.SetActive(true);

        _stopAutoPlay = true;
        var defaultSetting = SettingsManager.GetSetting(DefaultDifficultySetting, lowest);
        _typeDifficultyToggles[defaultSetting].SetIsOnWithoutNotify(true);
        SetSelectedDifficulty(_typeDifficultyToggles[defaultSetting]);
    }

    public void SetSelectedType(Toggle toggle)
    {
        SetSelectedType(toggle, true);
    }

    private void SetSelectedType(Toggle toggle, bool updateDisplay)
    {
        if (!toggle.isOn || _difficultySets.Length == 0)
        {
            return;
        }

        var toggleID = _gameTypeToggles.GetToggleID(toggle);

        if (toggleID < 0)
        {
            Debug.LogError("Song does not have matching toggle");
            return;
        }

        var gameModeID = -1;
        for (var i = 0; i < _difficultySets.Length; i++)
        {
            var gameMode = (int)_difficultySets[i].MapGameMode - 1;
            if (_difficultySets[i].MapGameMode == GameMode.Lawless)
            {
                gameMode--;
            }

            if (toggleID == gameMode)
            {
                gameModeID = i;
                break;
            }
        }

        if (gameModeID < 0)
        {
            Debug.LogError($"{_songInfo.SongName} does not have Game Mode: {(GameMode)toggleID + 1}");
            return;
        }

        _activeDifficultySet = _difficultySets[gameModeID];

        SettingsManager.SetSetting(DefaultGameModeSetting, gameModeID);
        UpdateAvailableDifficulties(updateDisplay);
    }

    public void SetSelectedDifficulty(Toggle toggle)
    {
        SetSelectedDifficulty(toggle, true);
    }

    private void SetSelectedDifficulty(Toggle toggle, bool updateDisplay)
    {
        if (!toggle.isOn || _activeDifficultySet.DifficultyInfos.Length == 0)
        {
            return;
        }

        var toggleID = _typeDifficultyToggles.GetToggleID(toggle);

        if (toggleID < 0)
        {
            Debug.LogError("Song does not have matching toggle");
            return;
        }
        var dificultyID = -1;
        for (var i = 0; i < _activeDifficultySet.DifficultyInfos.Length; i++)
        {
            var difficulty = (int)_activeDifficultySet.DifficultyInfos[i].DifficultyAsEnum - 1;
            if (difficulty == toggleID)
            {
                dificultyID = i;
                break;
            }
        }

        if (dificultyID < -0)
        {
            ErrorReporter.SetSuppressed(true);
            var difficulty = (DifficultyEnum)(toggleID + 1);
            Debug.LogError($"{_songInfo.SongName} does not have difficulty: {difficulty}");
            ErrorReporter.SetSuppressed(false);
            NotificationManager.RequestNotification(new Notification.NotificationVisuals($"{_songInfo.SongName} does not have maps for difficulty: {difficulty}. Lowest difficulty selected.",
                                                                                            "Difficulty Unavailable",
                                                                                            "Ignore", "Delete Song",
                                                                                            disableUI: true,
                                                                                            popUp: true), null, () =>
                                                                                            {
                                                                                                AssetManager.DeleteCustomSong(_songInfo);
                                                                                                HideOptions();
                                                                                            });
            dificultyID = (int)_activeDifficultySet.DifficultyInfos[0].DifficultyAsEnum - 1;
        }

        _selectedDifficulty = _activeDifficultySet.DifficultyInfos[dificultyID].Difficulty;
        _difficultyEnum = _activeDifficultySet.DifficultyInfos[dificultyID].DifficultyAsEnum;

        SettingsManager.SetSetting(DefaultDifficultySetting, dificultyID);

        if (!updateDisplay)
        {
            return;
        }
        _songRecordsDisplay?.RefreshDisplay();
    }

    public void SetSelectedDifAndMode(DifficultyInfo.DifficultyEnum difficulty, GameMode mode)
    {
        var difficultyToggle = _typeDifficultyToggles[(int)difficulty - 1];
        var modeToggle = _gameTypeToggles[(int)mode - 1];

        modeToggle.SetIsOnWithoutNotify(true);
        SetSelectedType(modeToggle, false);

        difficultyToggle.SetIsOnWithoutNotify(true);
        SetSelectedDifficulty(difficultyToggle, false);

        _songRecordsDisplay?.RefreshDisplay();
    }

    public void AddSelectedPlaylistItem()
    {
        if (PlaylistMaker.Instance != null)
        {
            var playlistItem = PlaylistMaker.GetPlaylistItem(_songInfo, _selectedDifficulty, _difficultyEnum, _activeDifficultySet.MapGameMode);
            PlaylistMaker.Instance.AddPlaylistItem(playlistItem);
        }
    }

    public void UpdateSelectedPlaylistItem()
    {
        if (PlaylistMaker.Instance != null)
        {

            var playlistItem = PlaylistMaker.GetPlaylistItem(_songInfo, _selectedDifficulty, _difficultyEnum, _activeDifficultySet.MapGameMode);
            PlaylistMaker.Instance.UpdatePlaylistItem(playlistItem);
        }
    }

    public void RemoveSelectedPlaylistItem()
    {
        if (PlaylistMaker.Instance != null)
        {
            PlaylistMaker.Instance.RemoveActivePlaylistItem();
        }
    }

    public void PlayIndividualSong()
    {
        if (PlaylistManager.Instance != null)
        {
            var oneHanded = PlaylistMaker.Instance.ForceOneHanded;
            var noObstacles = PlaylistMaker.Instance.ForceNoObstacles;
            var jabsOnly = PlaylistMaker.Instance.ForceJabsOnly;
            var targetSpeedMod = PlaylistMaker.Instance.TargetSpeedMod;
            var playlistItem = new PlaylistItem(_songInfo, _selectedDifficulty, _difficultyEnum, _activeDifficultySet.MapGameMode, noObstacles, oneHanded, jabsOnly);
            var targetSideType = _forwardFootSetter?.TargetHitSideType ?? HitSideType.Right;
            PlaylistManager.Instance.SetTempSongPlaylist(playlistItem, targetSideType, noObstacles, oneHanded, jabsOnly, targetSpeedMod);
            if (_autoPlayPreview)
            {
                StopSongPreview();
            }
        }
    }

    public void ToggleSongPreview()
    {
        if (_audioSource.isPlaying)
        {
            StopSongPreview();
            return;
        }
        PlaySongAudioAsync().Forget();
    }

    public void ToggleIfSameSong(SongInfo info)
    {
        if (_audioSource.isPlaying)
        {
            StopSongPreview();
        }
        if (_songInfo == info)
        {
            return;
        }
        else
        {
            UpdateDifficultyOptions(info, info.DifficultySets);
        }
        PlaySongAudioAsync().Forget();
    }

    public void PreviewSong()
    {
        if (_loadingSongPreview || _audioSource.isPlaying)
        {
            StopSongPreview();
        }
        PlaySongAudioAsync().Forget();
    }

    public async void StopSongPreview()
    {
        _audioSource.Stop();
        _previewButtonText?.SetTextZeroAlloc(Preview, true);
        if (_currentSongRequestHandle.IsValid())
        {
            Addressables.Release(_currentSongRequestHandle);
        }
        if (_cancellationSource != null && _cancellationSource.IsCancellationRequested)
        {
            _cancellationSource.Dispose();
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
            _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
        }
        else
        {
            RefreshTokenAsync().Forget();
        }
        _loadingSongPreview = false;
    }

    private async UniTaskVoid RefreshTokenAsync()
    {
        _cancellationSource?.Cancel();
        await UniTask.DelayFrame(1);
        _cancellationSource?.Dispose();
        _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
    }

    private async UniTaskVoid PlaySongAudioAsync()
    {
        try
        {

            AudioClip audioClip;
            if (_cancellationSource.IsCancellationRequested)
            {
                _cancellationSource.Dispose();
                if (_cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                await UniTask.DelayFrame(1);
                _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
            }
            _previewButtonText?.SetTextZeroAlloc(Loading, true);
            _loadingSongPreview = true;

            if (_songInfo.isCustomSong)
            {
                audioClip = await AssetManager.LoadCustomSong(_songInfo.fileLocation, _songInfo, _cancellationSource.Token);
            }
            else
            {
                var clipRequest = await AssetManager.LoadBuiltInSong(_songInfo, _cancellationSource.Token);
                _currentSongRequestHandle = clipRequest.OperationHandle;
                audioClip = clipRequest.AudioClip;
            }
            await UniTask.DelayFrame(1).SuppressCancellationThrow();
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

            _previewButtonText?.SetTextZeroAlloc(Stop, true);
            _loadingSongPreview = false;
            await UniTask.WaitUntil(() => _audioSource == null || _audioSource.isPlaying, cancellationToken: _cancellationSource.Token);

            if (_cancellationSource.IsCancellationRequested)
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                StopSongPreview();
                return;
            }

            await UniTask.WaitUntil(() => _audioSource == null || !_audioSource.isPlaying && FocusTracker.Instance.IsFocused, cancellationToken: _cancellationSource.Token);

            if (_cancellationToken.IsCancellationRequested)
            {
                return;
            }

            StopSongPreview();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (ex is not ObjectDisposedException)
            {
                Debug.LogError(ex);
            }
        }
    }

    private async UniTaskVoid WaitAndPlayPreview()
    {
        try
        {
            await UniTask.DelayFrame(2, cancellationToken: _cancellationToken);
            var activeMap = _songInfo;
            await UniTask.Delay(TimeSpan.FromSeconds(2.5f), cancellationToken: _cancellationSource.Token);
            if (_cancellationSource.IsCancellationRequested || activeMap != _songInfo)
            {
                return;
            }
            PreviewSong();
        }
        catch (Exception e)
        {
            if (e is OperationCanceledException or ObjectDisposedException || _cancellationSource == null)
            {
                return;
            }

            Debug.LogError($"failed to play song preview\n {e.Message}");
        }
    }
}
