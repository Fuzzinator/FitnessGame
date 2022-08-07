using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
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

    private const string DIVIDER = ":";
    public float Length => _length;

    public string ReadableLength
    {
        get
        {
            var minutes = (int)Mathf.Floor(_length / MINUTE);
            var seconds = (int)Mathf.Floor(_length % MINUTE);
            using (var sb = ZString.CreateStringBuilder(true))
            {
                if (minutes < 10)
                {
                    sb.Append(0);
                }
                sb.Append(minutes);
                sb.Append(DIVIDER);
                if (seconds < 10)
                {
                    sb.Append(0);
                }
                sb.Append(seconds);
                
                //var buffer = sb.AsArraySegment();
                return sb.ToString();
            }
        }
    }

    public string PlaylistName => _playlistName;

    [FormerlySerializedAs("items")] [SerializeField]
    private PlaylistItem[] _items;

    public PlaylistItem[] Items => _items;

    [SerializeField]
    private bool _isCustomPlaylist;

    [NonSerialized]
    public bool isValid;
    
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

        isValid = true;
    }

    public Playlist(PlaylistItem singleSong)
    {
        _playlistName = singleSong.SongName;
        _items = new[] {singleSong};
        _isCustomPlaylist = singleSong.IsCustomSong;
        _length = singleSong.SongInfo.SongLength;
        isValid = true;
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