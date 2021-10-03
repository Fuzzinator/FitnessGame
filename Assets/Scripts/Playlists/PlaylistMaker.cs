using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlaylistMaker : MonoBehaviour
{
    public static PlaylistMaker Instance { get; private set; }

    private Playlist _newPlaylist;
    public List<PlaylistItem> _playlistItems = new List<PlaylistItem>();
    private SongInfo _activeItem;
    
    [SerializeField]
    private UnityEvent _playlistItemsUpdated = new UnityEvent();
    
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

    public void SetActiveItem(SongInfo info)
    {
        _activeItem = info;
    }

    public PlaylistItem GetPlaylistItem(string difficulty)
    {
        return new PlaylistItem(_activeItem.SongName, _activeItem.fileLocation, difficulty, _activeItem.isCustomSong);
    }

    public void AppendPlaylistItems(PlaylistItem item)
    {
        if (_playlistItems.Contains(item))
        {
            _playlistItems.Remove(item);
        }
        else
        {
            _playlistItems.Add(item);
        }
        _playlistItemsUpdated?.Invoke();
    }
}
