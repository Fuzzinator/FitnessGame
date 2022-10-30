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
    
    public UnityEvent songCompleted = new UnityEvent();
    public UnityEvent prepForNextSong = new UnityEvent();
    public UnityEvent levelLoadFailed = new UnityEvent();

    public UnityEvent restartingLevel = new UnityEvent();
    public UnityEvent levelCompleted = new UnityEvent();

    [SerializeField]
    private int _delayLength = 5;

    [SerializeField]
    private bool _choreographyLoaded = false;
    
    [SerializeField]
    private bool _songInfoLoaded = false;
    
    [SerializeField]
    private bool _actualSongLoaded = false;

    private bool _canComplete = false;
    private bool _choreographyCompleted = false;
    private bool _songCompleted = false;

    private UniTask _songCountdown;
    private CancellationTokenSource _cancellationToken;
    public bool SongFullyLoaded => _choreographyLoaded && _songInfoLoaded && _actualSongLoaded;

    public bool SongCompleted => _songCompleted;

    public bool ChoreographyLoaded
    {
        get => _choreographyLoaded;
        private set
        {
            _choreographyLoaded = value;
            
            CheckIfLoaded().Forget();
        }
    }
    
    public bool SongInfoLoaded
    {
        get => _songInfoLoaded;
        private set
        {
            _songInfoLoaded = value;
            
            CheckIfLoaded().Forget();
        }
    }

    public bool ActualSongLoaded
    {
        get => _actualSongLoaded;
        private set
        {
            _actualSongLoaded = value;
            
            CheckIfLoaded().Forget();
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
        
        _cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        if (PlaylistManager.Instance != null)
        {
            startedLevelLoad.AddListener(PlaylistManager.Instance.SetFirstPlaylistItem);
        }
        InputManager.Instance.EnableActionMaps("In Game");
        
        if (PlaylistManager.Instance != null)
        {
            prepForNextSong.AddListener(PlaylistManager.Instance.UpdateCurrentPlaylist);
        }
        LoadLevel();
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Restart()
    {
        restartingLevel?.Invoke();
        LoadLevel();
    }

    public void LoadLevel()
    {
        ResetForNextSong();
        startedLevelLoad?.Invoke();
    }

    public void LoadFailed()
    {
        levelLoadFailed?.Invoke();
        ResetForNextSong();
    }

    public void LoadNextSong()
    {
        LoadFailed();
        
        PlaylistManager.Instance.UpdateCurrentPlaylist();
    }

    public void ResetForNextSong()
    {
        _choreographyLoaded = false;
        _songInfoLoaded = false;
        _actualSongLoaded = false;
    }

    public void SetChoreographyLoaded(bool loaded)
    {
        if (ChoreographyLoaded == loaded)
        {
            return;
        }
        ChoreographyLoaded = loaded;
        _choreographyCompleted = false;
    }

    public void SetSongInfoLoaded(bool loaded)
    {
        if (SongInfoLoaded == loaded)
        {
            return;
        }
        SongInfoLoaded = loaded;
    }

    public void SetActualSongLoaded(bool loaded)
    {
        if (ActualSongLoaded == loaded)
        {
            return;
        }
        ActualSongLoaded = loaded;
        _songCompleted = false;
    }

    public void SetChoreographyCompleted(bool completed)
    {
        _choreographyCompleted = _choreographyLoaded;
        CheckIfCompleted();
    }

    public void SetActualSongCompleted(bool completed)
    {
        _songCompleted = completed;
        CheckIfCompleted();
    }

    private async UniTask CheckIfLoaded()
    {
        if (_choreographyLoaded && _songInfoLoaded && _actualSongLoaded)
        {
            finishedLevelLoad?.Invoke(_delayLength);
            _songCountdown = DelaySongStart(_delayLength);
            await _songCountdown.SuppressCancellationThrow();
        }
    }

    private void CheckIfCompleted()
    {
        if (_choreographyCompleted && _songCompleted && _canComplete)
        {
            _canComplete = false;
            FireEndSongMessagesAsync().Forget();
        }
    }

    private async UniTaskVoid FireEndSongMessagesAsync()
    {
        songCompleted?.Invoke();
        await UniTask.DelayFrame(1, cancellationToken:_cancellationToken.Token);
        if (PlaylistManager.Instance == null || PlaylistManager.Instance.CurrentPlaylist.Items == null)
        {
            return;
        }
        
        prepForNextSong?.Invoke();
        
        if (PlaylistManager.Instance.CurrentIndex == PlaylistManager.Instance.CurrentPlaylist.Items.Length - 1)
        {
            levelCompleted?.Invoke();
        }
    }

    private async UniTask DelaySongStart(float delayLength)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delayLength), cancellationToken: _cancellationToken.Token,
            ignoreTimeScale: false);
        if (_cancellationToken.IsCancellationRequested)
        {
            return;
        }

        PlayLevel();
        _canComplete = true;
    }

    private void PlayLevel()
    {
        playLevel?.Invoke();
    }

    public void CancelLevelLoad()
    {
        _cancellationToken.Cancel(false);
    }
}