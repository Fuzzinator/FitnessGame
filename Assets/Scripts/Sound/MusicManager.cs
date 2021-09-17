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
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null && InputManager.Instance.MainInput != null)
        {
            foreach (var action in InputManager.Instance.MainInput.actions)
            {
                switch (action.name)
                {
                    case "Select":
                        action.started += TempStart;
                        break;
                    case "Menu Button":
                        action.started += ToggleMusic;
                        break;
                }
            }
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null && InputManager.Instance.MainInput != null)
        {
            foreach (var action in InputManager.Instance.MainInput.actions)
            {
                switch (action.name)
                {
                    case "Select":
                        action.started -= TempStart;
                        break;
                    case "Menu Button":
                        action.started -= ToggleMusic;
                        break;
                }
            }
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

    private async UniTaskVoid AsyncLoadFromPlaylist(PlaylistItem info)
    {
        var path = $"{Application.dataPath}\\Resources\\{info.FileLocation}\\song.ogg";
        using (var uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.OGGVORBIS))
        {
            uwr.SendWebRequest();
            await UniTask.WaitUntil(() => uwr.isDone);
            try
            {
                if (uwr.isDone && uwr.result == UnityWebRequest.Result.Success)
                {
                    var clip = DownloadHandlerAudioClip.GetContent(uwr);
                    clip.name = info.SongName;
                    SetNewMusic(clip);
                    finishedLoadingSong?.Invoke();
                }
            }
            catch (Exception error)
            {
                Debug.LogWarning($"{error.Message}\n{error.StackTrace}");
            }
        }
    }

    public void StartNewSequence()
    {
        var music = SongInfoReader.Instance.GetCurrentSong();
        SetNewMusic(music);
        Instance.PlayMusic();
    }

    public void SetNewMusic(AudioClip song)
    {
        _musicAudioSource.clip = song;
    }

    public void PlayMusic()
    {
        _musicAudioSource.Play();
#pragma warning disable 4014
        WaitForSongFinish();
#pragma warning restore 4014
    }

    public void PauseMusic()
    {
        _musicAudioSource.Pause();
    }

    public void StopMusic()
    {
        _musicAudioSource.Stop();
    }

    public void ToggleMusic(InputAction.CallbackContext context)
    {
        if (_musicAudioSource.isPlaying)
        {
            PauseMusic();
        }
        else
        {
            PlayMusic();
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
        
        songFinishedPlaying?.Invoke();
    }
}