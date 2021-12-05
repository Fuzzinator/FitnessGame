using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

[Serializable]
public struct Playlist
{
    [SerializeField]
    private string _playlistName;

    //workout length in seconds
    [SerializeField]
    private float _length;

    public float Length => _length;

    public string ReadableLength
    {
        get
        {
            var minutes = Mathf.Floor(_length / MINUTE);
            var seconds = Mathf.Floor(_length % MINUTE);
            return $"{minutes}:{seconds:00}";
        }
    }

    public string PlaylistName => _playlistName;

    [FormerlySerializedAs("items")] [SerializeField]
    private PlaylistItem[] _items;

    public PlaylistItem[] Items => _items;

    [SerializeField]
    private bool _isCustomPlaylist;

    public bool IsCustomPlaylist => _isCustomPlaylist;

    private const int MINUTE = 60;

    public Playlist(List<PlaylistItem> items, string playlistName = null, bool isCustomPlaylist = true)
    {
        _playlistName = string.IsNullOrWhiteSpace(playlistName)
            ? $"{DateTime.Now:yyyy-MM-dd} - {DateTime.Now:hh-mm}"
            : playlistName;
        _items = items.ToArray();
        _isCustomPlaylist = isCustomPlaylist;
        _length = 0;
        foreach (var item in items)
        {
            _length += item.SongInfo.SongLength;
        }
    }

    public void SetPlaylistName(string name)
    {
        _playlistName = name;
    }

    public void ShuffleItems()
    {
        _items.Shuffle();
    }
    
    public enum SortingMethod
    {
        None = 0,
        PlaylistName = 1,
        InversePlaylistName = 2,
        PlaylistLength = 5,
        InversePlaylistLength = 6
    }
}