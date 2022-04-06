using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SoundObject : MonoBehaviour, IPoolable
{
    [SerializeField]
    private AudioSource _audioSource;

    private bool _isPaused = false;
    private bool _applicationPaused = false;
    private float _previousTime;
    private SoundManager.AudioSourceSettings _settings;
    
    public PoolManager MyPoolManager { get; set; }
    public bool IsPooled { get; set; }

    private bool IsPlayingOrPaused => _audioSource.isPlaying || _isPaused || _applicationPaused;

    private bool IsSoundCompleted => !_settings.Looping && _previousTime > 0 && _audioSource.time == 0;
    
    public void Initialize()
    {
        StartMonitoring(this.GetCancellationTokenOnDestroy()).Forget();
    }

    public void Play(AudioClip audioClip, SoundManager.AudioSourceSettings settings)
    {
        _previousTime = 0;
        SetAudioSourceSettings(settings);
        _audioSource.clip = audioClip;
        Resume();
    }

    private void SetAudioSourceSettings(SoundManager.AudioSourceSettings settings)
    {
        _settings = settings;
        _audioSource.loop = settings.Looping;
        _audioSource.volume = settings.InitialVolume;
        _audioSource.outputAudioMixerGroup = settings.MixerGroup;
    }

    public void SetVolume(float volume)
    {
        _audioSource.volume = volume;
    }

    public void Resume()
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
        
        _audioSource.Play();
        _isPaused = false;
    }

    public void Pause()
    {
        _audioSource.Pause();
        _isPaused = true;
    }

    public void Stop()
    {
        _audioSource.Stop();
        SetAudioSourceSettings(new SoundManager.AudioSourceSettings());
        _isPaused = false;
    }


    public void ToggleSound(bool play)
    {
        if (play && _isPaused)
        {
            Resume();
        }
        else if (!play && !_isPaused)
        {
            Pause();
        }
    }
    
    public void ReturnToPool()
    {
        _audioSource.Stop();
        SoundManager.Instance.ReturnToPool(this);
        gameObject.SetActive(false);
    }

    private async UniTaskVoid StartMonitoring(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(.1f), cancellationToken: token);
            if (_previousTime < 0)
            {
                continue;
            }
            
            if (_settings.Looping || !IsSoundCompleted && IsPlayingOrPaused)
            {
                _previousTime = _audioSource.time;
                continue;
            }

            _previousTime = -1;

            if (token.IsCancellationRequested)
            {
                return;
            }
            ReturnToPool();
        }
        ReturnToPool();
    }
}