using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

public class MusicManager : BaseGameStateListener
{
    public static MusicManager Instance { get; private set; }

    [SerializeField]
    private AudioSource _musicAudioSource;

    public UnityEvent finishedLoadingSong = new UnityEvent();

    public UnityEvent songFinishedPlaying = new UnityEvent();

    private CancellationTokenSource _cancellationSource;
    private CancellationToken _cancellationToken;

    private bool _awaitingSongEnd = false;
    private bool _musicPaused = false;

    private SongLoader _songLoader;

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
        _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        _cancellationToken = this.GetCancellationTokenOnDestroy();

        _songLoader = new SongLoader();
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
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
            _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        }
        
        if (item.IsCustomSong)
        {
            audioClip = await _songLoader.LoadCustomSong(item.FileLocation, item.SongInfo, _cancellationSource.Token);
        }
        else
        {
            audioClip = await _songLoader.LoadBuiltInSong(item.SongInfo, _cancellationSource.Token);
        }
        
        
        if (audioClip == null)
        {
            LevelManager.Instance.LoadFailed();
            NotificationManager.ReportFailedToLoadInGame($"{item.SongName}'s music failed to load.");
            if (_cancellationSource.IsCancellationRequested)
            {
                _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
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
            if (SongInfoReader.Instance.songInfo.SongStartDelay > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(SongInfoReader.Instance.songInfo.SongStartDelay),
                    cancellationToken: _cancellationToken);
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
        _musicAudioSource.Stop();
        _musicPaused = false;
    }

    protected override void GameStateListener(GameState oldState, GameState newState)
    {
        if (oldState == GameState.Paused && newState == GameState.Playing)
        {
            ToggleMusic(true);
        }
        else if (oldState == GameState.Playing && (newState == GameState.Paused || newState == GameState.Unfocused))
        {
            ToggleMusic(false);
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
            var isMusicPlaying = _musicAudioSource.isPlaying || _musicPaused;
            while (_musicAudioSource.clip.length - _musicAudioSource.time >= .0525f && isMusicPlaying)
            {
                await UniTask.Delay(timeSpan, cancellationToken: _cancellationToken);
            }
        }

        if (_cancellationToken.IsCancellationRequested)
        {
            return;
        }

        _awaitingSongEnd = false;
        songFinishedPlaying?.Invoke();
        LevelManager.Instance.SetActualSongCompleted(true);
    }
}