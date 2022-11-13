using System;
using System.Collections;
using System.Collections.Generic;
using GameModeManagement;
using UnityEngine;
using UnityEngine.Events;

public class PlaylistManager : MonoBehaviour
{
    public static PlaylistManager Instance { get; private set; }

    private PlaylistItem _currentItem;

    public PlaylistItem CurrentItem
    {
        get => _currentItem;

        set
        {
            //TODO:Make a better way for scene changing so this can be un-commented out
            //if (_currentItem != value)
            //{
            _currentItem = value;
            playlistItemUpdated?.Invoke(value);
            //}
            //else
            //{
            //    Debug.Log("_currentItem already set?");
            //}
        }
    }

    [SerializeField]
    private Playlist _currentPlaylist;

    public Playlist CurrentPlaylist
    {
        get => _currentPlaylist;
        set
        {
            _currentPlaylist = value;
            currentPlaylistUpdated?.Invoke(value);
        }
    }
    
    public UnityEvent<PlaylistItem> playlistItemUpdated = new UnityEvent<PlaylistItem>();
    public UnityEvent<Playlist> currentPlaylistUpdated = new UnityEvent<Playlist>();

    private int _currentIndex = 0;

    public int CurrentIndex => _currentIndex;

    public int SongCount => _currentPlaylist?.Items?.Length ?? 0;
    
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

    public void SetActivePlaylist(Playlist playlist)
    {
        CurrentPlaylist = playlist;
    }

    public void SetFirstPlaylistItem()
    
    {
        _currentIndex = 0;
        if (_currentPlaylist?.Items == null || _currentPlaylist.Items.Length <= _currentIndex)
        {
            Debug.LogError("Playlist has no items");
            return;
        }

        CurrentItem = _currentPlaylist.Items[_currentIndex];
    }

    public void UpdateCurrentPlaylist()
    {
        _currentIndex++;

        if (_currentPlaylist?.Items == null || _currentPlaylist.Items.Length == 0 ||
            _currentIndex >= _currentPlaylist.Items.Length)
        {
            return;
        }
        
        CurrentItem = _currentPlaylist.Items[_currentIndex];
    }

    public void Restart()
    {
        _currentIndex = -1;
    }
    
    public void FullReset()
    {
        _currentIndex = 0;
        _currentItem = new PlaylistItem();
        _currentPlaylist = null;
    }

    public void SetTempSongPlaylist(PlaylistItem playlistItem)
    {
        var tempPlaylist = new Playlist(playlistItem);
        CurrentPlaylist = tempPlaylist;
    }

    public void SetEnvironment(string envName)
    {
        _currentPlaylist?.SetEnvironment(envName);// = new Playlist(_currentPlaylist, envName);
    }

    public void SetDifficulty(DifficultyInfo.DifficultyEnum difficultyEnum)
    {
        _currentPlaylist?.SetDifficulty(difficultyEnum);// = new Playlist(_currentPlaylist, difficultyEnum);
    }

    public void SetGameMode(GameMode gameMode)
    {
        _currentPlaylist?.SetGameMode(gameMode);// = new Playlist(_currentPlaylist, gameMode);
    }
}