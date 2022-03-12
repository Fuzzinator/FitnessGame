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
    public PoolManager MyPoolManager { get; set; }
    public bool IsPooled { get; set; }

    private bool IsPlayingOrPaused => _audioSource.isPlaying || _isPaused || _applicationPaused;

    private bool IsSoundCompleted => _previousTime > 0 && _audioSource.time == 0;
    
    public void Initialize()
    {
        StartMonitoring(this.GetCancellationTokenOnDestroy()).Forget();
    }

    public void Play(AudioClip audioClip)
    {
        _previousTime = 0;
        _audioSource.clip = audioClip;
        Resume();
    }

    public void Resume()
    {
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
            
            if (!IsSoundCompleted && IsPlayingOrPaused)
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