using System;
using System.Collections;
using System.Collections.Generic;
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
            currentPlaylistUpdated?.Invoke();
        }
    }

    public UnityEvent<PlaylistItem> playlistItemUpdated = new UnityEvent<PlaylistItem>();
    public UnityEvent currentPlaylistUpdated = new UnityEvent();

    private int _currentIndex = 0;

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
        if (_currentPlaylist.Items == null || _currentPlaylist.Items.Length <= _currentIndex)
        {
            Debug.LogError("Playlist has no items");
            return;
        }
        CurrentItem = _currentPlaylist.Items[_currentIndex];
    }

    public void UpdateCurrentPlaylist()
    {
        if (_currentPlaylist.Items == null || _currentPlaylist.Items.Length == 0 || _currentIndex >= _currentPlaylist.Items.Length - 1)
        {
            return;
        }
        _currentIndex++;
        CurrentItem = _currentPlaylist.Items[_currentIndex];
    }
}