using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public UnityEvent startedLevelLoad = new UnityEvent();
    public UnityEvent<int> finishedLevelLoad = new UnityEvent<int>();
    public UnityEvent playLevel = new UnityEvent();

    [SerializeField]
    private int _delayLength = 5;
    
    private bool _choreographyLoaded = false;
    private bool _songInfoLoaded = false;
    private bool _actualSongLoaded = false;

    private CancellationToken _cancellationToken;
    public bool ChoreographyLoaded
    {
        get => _choreographyLoaded;
        private set
        {
            _choreographyLoaded = value;
            CheckIfLoaded();
        }
    }
    public bool SongInfoLoaded
    {
        get => _songInfoLoaded;
        private set
        {
            _songInfoLoaded = value;
            CheckIfLoaded();
        }
    }
    public bool ActualSongLoaded
    {
        get => _actualSongLoaded;
        private set
        {
            _actualSongLoaded = value;
            CheckIfLoaded();
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
        ResetForNextSong();
        startedLevelLoad?.Invoke();
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    public void ResetForNextSong()
    {
        _choreographyLoaded = false;
        _songInfoLoaded = false;
        _actualSongLoaded = false;
    }

    public void SetChoreographyLoaded(bool loaded)
    {
        ChoreographyLoaded = loaded;
    }

    public void SetSongInfoLoaded(bool loaded)
    {
        SongInfoLoaded = loaded;
    }

    public void SetActualSongLoaded(bool loaded)
    {
        ActualSongLoaded = loaded;
    }

    private async UniTask CheckIfLoaded()
    {
        if (_choreographyLoaded && _songInfoLoaded && _actualSongLoaded)
        {
            finishedLevelLoad?.Invoke(_delayLength);
            await DelaySongStart();
        }
    }

    private async UniTask DelaySongStart()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(_delayLength), cancellationToken: _cancellationToken);
        if (_cancellationToken.IsCancellationRequested)
        {
            return;
        }

        PlayLevel();
    }

    private void PlayLevel()
    {
        playLevel?.Invoke();
    }
}