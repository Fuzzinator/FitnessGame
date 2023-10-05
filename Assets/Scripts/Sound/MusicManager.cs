using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MusicManager : BaseGameStateListener
{
    public static MusicManager Instance { get; private set; }

    [SerializeField]
    private AudioSource _musicAudioSource;

    public UnityEvent finishedLoadingSong = new UnityEvent();

    public UnityEvent songFinishedPlaying = new UnityEvent();

    private CancellationTokenSource _cancellationSource;
    private CancellationToken _cancellationToken;
    private AsyncOperationHandle _currentSongRequestHandle = new AsyncOperationHandle();

    private bool _awaitingSongEnd = false;
    private bool _musicPaused = false;
    private bool _applicationPaused = false;

    private float _previousTime = 0;

    public bool IsPlaying => _musicAudioSource.isPlaying;

    public bool IsPaused => _musicPaused || _applicationPaused;

    private bool IsPlayingOrPaused => _musicAudioSource.isPlaying || _musicPaused || _applicationPaused;

    private bool IsSongCompleted => _previousTime > 0 && _musicAudioSource.time == 0;

    #region Const Strings

    private const string SELECT = "Select";
    private const string MENUBUTTON = "Menu Button";
#if UNITY_EDITOR
    private const string PAUSEINEDITOR = "Pause In Editor";
#endif
    private const string LOCALSONGSFOLDER = "Assets/Music/Songs/";

    #endregion

    private void OnValidate()
    {
        if (_musicAudioSource == null)
        {
            TryGetComponent(out _musicAudioSource);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
        _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        _applicationPaused = pauseStatus;
    }

    public async void LoadFromPlaylist(PlaylistItem info)
    {
        await AsyncLoadFromPlaylist(info);
    }

    public void CancelLoad()
    {
        _cancellationSource?.Cancel();
    }

    private async UniTask AsyncLoadFromPlaylist(PlaylistItem item)
    {
        AudioClip audioClip;
        if (_cancellationSource.IsCancellationRequested)
        {
            _cancellationSource.Dispose();
            _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        }

        if (item.IsCustomSong)
        {
            audioClip = await AssetManager.LoadCustomSong(item.FileLocation, item.SongInfo, _cancellationSource.Token);
        }
        else
        {
            var clipRequest = await AssetManager.LoadBuiltInSong(item.SongInfo, _cancellationSource.Token);
            _currentSongRequestHandle = clipRequest.OperationHandle;
            audioClip = clipRequest.AudioClip;
        }


        if (audioClip == null)
        {
            LevelManager.Instance.LoadFailed();
            NotificationManager.ReportFailedToLoadInGame($"{item.SongName}'s music failed to load.");
            if (_cancellationSource.IsCancellationRequested)
            {
                _cancellationSource.Dispose();
                _cancellationSource =
                    CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            }

            return;
        }

        SetNewMusic(audioClip);

        finishedLoadingSong?.Invoke();
    }

    public void StartNewSequence()
    {
        PlayMusic();
    }

    public void SetNewMusic(AudioClip song)
    {
        _musicAudioSource.clip = song;
    }

    public async void PlayMusic()
    {
        _musicAudioSource.Play();
        _musicPaused = false;
        LevelManager.Instance.SetActualSongCompleted(false);
        if (!_awaitingSongEnd)
        {
            _awaitingSongEnd = true;
            await WaitForSongFinish().SuppressCancellationThrow();
        }
    }

    public async void WaitThenPlayMusic()
    {
        try
        {
            _cancellationSource?.Dispose();
            _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
            if (SongInfoReader.Instance.songInfo.SongStartDelay > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(SongInfoReader.Instance.songInfo.SongStartDelay),
                    cancellationToken: _cancellationSource.Token);
                if (_cancellationSource.IsCancellationRequested)
                {
                    return;
                }
            }
            
            _musicAudioSource.Play();
            _musicPaused = false;
            LevelManager.Instance.SetActualSongCompleted(false);
            if (_awaitingSongEnd)
            {
                return;
            }

            _awaitingSongEnd = true;
            await WaitForSongFinish();
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
            return;
        }
    }

    public void PauseMusic()
    {
        _musicAudioSource.Pause();
        _musicPaused = true;
    }

    public void StopMusic()
    {
        _cancellationSource?.Cancel();
        _musicAudioSource.Stop();
        _musicPaused = false;
        _awaitingSongEnd = false;
        if(_currentSongRequestHandle.IsValid())
        {
            Addressables.Release(_currentSongRequestHandle);
        }
    }

    public float GetSongPercentage()
    {
        var totalLength = _musicAudioSource.clip.length;
        var currentPosition = _musicAudioSource.time;
        return currentPosition / totalLength;
    }

    protected override void GameStateListener(GameState oldState, GameState newState)
    {
        switch (oldState)
        {
            case GameState.Paused when newState == GameState.Playing:
                ToggleMusic(true);
                break;
            case GameState.Playing when (newState == GameState.Paused || newState == GameState.Unfocused):
                ToggleMusic(false);
                break;
        }
    }

    public void ToggleMusic(bool play)
    {
        if (LevelManager.Instance == null || !LevelManager.Instance.SongFullyLoaded || !_awaitingSongEnd ||
            LevelManager.Instance.SongCompleted)
        {
            return;
        }

        if (play && _musicPaused)
        {
            PlayMusic();
        }
        else if (!play && !_musicPaused)
        {
            PauseMusic();
        }
    }

    private async UniTask WaitForSongFinish()
    {
        if (_musicAudioSource != null && _musicAudioSource.clip != null)
        {
            var timeSpan = TimeSpan.FromSeconds(.05f);
            _previousTime = -1f;
            while (!IsSongCompleted && IsPlayingOrPaused && !_cancellationSource.IsCancellationRequested)//_musicAudioSource.clip.length - _musicAudioSource.time >= .05f && IsPlayingOrPaused)
            {
                _previousTime = _musicAudioSource.time;
                await UniTask.Delay(timeSpan, cancellationToken: _cancellationSource.Token);
            }

            _previousTime = -1f;
        }

        _awaitingSongEnd = false;
        if (_cancellationSource.IsCancellationRequested)
        {
            return;
        }

        songFinishedPlaying?.Invoke();
        LevelManager.Instance.SetActualSongCompleted(true);
    }
}