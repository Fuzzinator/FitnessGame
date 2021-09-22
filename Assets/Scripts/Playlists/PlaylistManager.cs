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
            if (_currentItem != value)
            {
                _currentItem = value;
                _playlistItemUpdated?.Invoke(value);
            }
        }
    }

    [SerializeField]
    private Playlist _currentPlaylist;
    
    [SerializeField]
    private UnityEvent<PlaylistItem> _playlistItemUpdated = new UnityEvent<PlaylistItem>();

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

    public void SetFirstPlaylistItem()
    {
        if (_currentPlaylist.Items == null || _currentPlaylist.Items.Length <= _currentIndex)
        {
            Debug.LogError($"CurrentPlaylist is {_currentPlaylist.Items}");
            return;
        }

        CurrentItem = _currentPlaylist.Items[_currentIndex];
    }
    
    public void UpdateCurrentPlaylist()
    {
        if (_currentPlaylist.Items == null || _currentPlaylist.Items.Length == 0 || _currentIndex >= _currentPlaylist.Items.Length-1)
        {
            return;
        }
        _currentIndex++;
        CurrentItem = _currentPlaylist.Items[_currentIndex];
    }
    
}
