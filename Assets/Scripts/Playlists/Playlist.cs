using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct Playlist
{
    [SerializeField]
    private string _playlistName;

    //workout length in seconds
    private float _length;
    public float ReadableLength
    {
        get
        {
            var minutes = _length / MINUTE;
            return (float)Math.Round(minutes, 2);
        }
    }
    public string PlaylistName => _playlistName;
    
    [FormerlySerializedAs("items")] [SerializeField]
    private PlaylistItem[] _items;
    public PlaylistItem[] Items => _items;

    private const int MINUTE = 60;

    public Playlist(List<PlaylistItem> items)
    {
        _playlistName = $"{DateTime.Now:yyyy-MM-dd} - {DateTime.Now:hh-mm}";
        _length = 0; //TODO:Make a good way to do this
        _items = items.ToArray();
    }
}
