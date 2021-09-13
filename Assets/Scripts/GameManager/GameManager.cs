using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public UnityEvent startedLevelLoad = new UnityEvent();
    public UnityEvent finishedLevelLoad = new UnityEvent();

    private bool _choreographyLoaded = false;
    private bool _songInfoLoaded = false;
    private bool _actualSongLoaded = false;

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
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        ResetForNextSong();
        startedLevelLoad?.Invoke();
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

    private void CheckIfLoaded()
    {
        if (_choreographyLoaded && _songInfoLoaded && _actualSongLoaded)
        {
            finishedLevelLoad?.Invoke();
        }
    }
}
