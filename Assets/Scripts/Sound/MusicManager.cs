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

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField]
    private AudioSource _musicAudioSource;

    public UnityEvent finishedLoadingSong = new UnityEvent();

    public UnityEvent songFinishedPlaying = new UnityEvent();

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
        _cancellationToken = this.GetCancellationTokenOnDestroy();

        _songLoader = new SongLoader();
    }

    private void OnEnable()
    {
        GameStateManager.Instance.gameStateChanged.AddListener(GameStateListener);
    }

    private void OnDisable()
    {
        GameStateManager.Instance.gameStateChanged.RemoveListener(GameStateListener);
    }

    public async void LoadFromPlaylist(PlaylistItem info)
    {
        await AsyncLoadFromPlaylist(info);
    }

    private async UniTask AsyncLoadFromPlaylist(PlaylistItem item)
    {
        if (item.IsCustomSong)
        {
            var clipRequest = _songLoader.LoadCustomSong(item.FileLocation, item.SongInfo);
            var task = clipRequest.AsTask();
            await task;
            /*if (clipRequest.Status != UniTaskStatus.Succeeded)//This doesnt work?
            {
                Debug.LogError($"LoadCustomSong failed. Result: {clipRequest.Status}");
                return;
            }*/
            SetNewMusic(task.Result);
        }
        else
        {
            var clipRequest = _songLoader.LoadBuiltInSong(item.SongInfo);
            var task = clipRequest.AsTask();
            await task;
            /*if (clipRequest.Status != UniTaskStatus.Succeeded)//This doesnt work?
            {
                Debug.LogError($"LoadBuiltInSong failed. Result: {clipRequest.Status}");
                return;
            }*/
            SetNewMusic(task.Result);
        }

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

    private void GameStateListener(GameState oldState, GameState newState)
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
        if (LevelManager.Instance == null || !LevelManager.Instance.SongFullyLoaded || !_awaitingSongEnd)
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
        await UniTask.WaitUntil(
            () => Math.Abs(_musicAudioSource != null && _musicAudioSource.clip != null
                ? _musicAudioSource.clip.length - _musicAudioSource.time
                : 1) < .1f, cancellationToken: _cancellationToken);

        if (_cancellationToken.IsCancellationRequested)
        {
            return;
        }

        _awaitingSongEnd = false;
        LevelManager.Instance.SetActualSongCompleted(true);
        songFinishedPlaying?.Invoke();
    }
}