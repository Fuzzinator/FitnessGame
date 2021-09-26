using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
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

    #region Const Strings

    private const string SELECT = "Select";
    private const string MENUBUTTON = "Menu Button";
#if UNITY_EDITOR
    private const string PAUSEINEDITOR = "Pause In Editor";
#endif
    
#if UNITY_ANDROID // && !UNITY_EDITOR
    private const string ANDROIDPATHSTART = "file://";
#endif

    private const string SONGSFOLDER = "/Resources/Songs/";
    private const string PLAYLISTEXTENSION = ".txt";

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
        
        if (PlaylistManager.Instance != null)
        {
            songFinishedPlaying.AddListener(PlaylistManager.Instance.UpdateCurrentPlaylist);
        }
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null && InputManager.Instance.MainInput != null)
        {
            InputManager.Instance.MainInput[SELECT].performed += TempStart;
            InputManager.Instance.MainInput[MENUBUTTON].performed += ToggleMusic;
            FocusTracker.Instance.focusChanged.AddListener(ToggleMusic);
#if UNITY_EDITOR
            InputManager.Instance.MainInput[PAUSEINEDITOR].performed += ToggleMusic;
#endif
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null && InputManager.Instance.MainInput != null)
        {
            InputManager.Instance.MainInput[SELECT].performed -= TempStart;
            InputManager.Instance.MainInput[MENUBUTTON].performed -= ToggleMusic;
            FocusTracker.Instance.focusChanged.RemoveListener(ToggleMusic);
#if UNITY_EDITOR
            InputManager.Instance.MainInput[PAUSEINEDITOR].performed -= ToggleMusic;
#endif
        }
    }

    private void TempStart(InputAction.CallbackContext context)
    {
        if (_musicAudioSource.isPlaying)
        {
            return;
        }

        StartNewSequence();
    }

    public void LoadFromPlaylist(PlaylistItem info)
    {
#pragma warning disable 4014
        AsyncLoadFromPlaylist(info);
#pragma warning restore 4014
    }

    private async UniTask AsyncLoadFromPlaylist(PlaylistItem item)
    {
        if (item.IsCustomSong)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var path =
 $"{ANDROIDPATHSTART}{Application.persistentDataPath}{SONGSFOLDER}{item.FileLocation}/{item.SongInfo.SongFilename}";
#elif UNITY_EDITOR
            var path = $"{Application.dataPath}{SONGSFOLDER}{item.FileLocation}/{item.SongInfo.SongFilename}";
#endif
            var uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.OGGVORBIS);
            await uwr.SendWebRequest();
            if (uwr.isDone && uwr.result == UnityWebRequest.Result.Success)
            {
                var clip = DownloadHandlerAudioClip.GetContent(uwr);
                clip.name = item.SongName;
                SetNewMusic(clip);
            }
            else
            {
                Debug.LogError("failed to get audio clip");
                return;
            }
        }
        else
        {
            var fileName = item.SongInfo.SongFilename.Substring(0, item.SongInfo.SongFilename.LastIndexOf('.'));
            var request = Resources.LoadAsync<AudioClip>($"Songs/{item.FileLocation}/{fileName}");
            await request;
            var clip = request.asset as AudioClip;
            if (clip == null)
            {
                Debug.LogError("Failed to load local resource file");
                return;
            }

            clip.name = item.SongName;
            SetNewMusic(clip);
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

    public void PlayMusic()
    {
        _musicAudioSource.Play();
        _musicPaused = false;
        if (!_awaitingSongEnd)
        {
#pragma warning disable 4014
            WaitForSongFinish();
#pragma warning restore 4014
            _awaitingSongEnd = true;
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

    public void ToggleMusic(InputAction.CallbackContext context)
    {
        if (_musicAudioSource.isPlaying)
        {
            PauseMusic();
        }
        else if (_musicPaused)
        {
            PlayMusic();
        }
    }

    public void ToggleMusic(bool play)
    {
        if (GameManager.Instance.GameIsPaused || LevelManager.Instance == null ||
            !LevelManager.Instance.SongFullyLoaded)
        {
            return;
        }

        if (play && _musicPaused)
        {
            PlayMusic();
        }
        else if (!_musicPaused)
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
        songFinishedPlaying?.Invoke();
    }
}