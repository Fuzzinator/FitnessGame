using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using GameModeManagement;
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

    [SerializeField]
    private PlaylistItem[] _items;

    public PlaylistItem[] Items => _items;

    [SerializeField]
    private bool _isCustomPlaylist;

    [SerializeField]
    private GameMode _gameMode;

    [SerializeField]
    private DifficultyInfo.DifficultyEnum _difficulty;

    [SerializeField]
    private string _targetEnvName;
    
    [NonSerialized]
    public bool isValid;
    
    public bool IsCustomPlaylist => _isCustomPlaylist;
    public float Length => _length;
    public string PlaylistName => _playlistName;

    public string TargetEnvName => _targetEnvName;

    public GameMode GameModeOverride => _gameMode;

    public DifficultyInfo.DifficultyEnum DifficultyEnum => _difficulty;

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

    private const int MINUTE = 60;
    private const string DIVIDER = ":";
    
    public Playlist(List<PlaylistItem> items, GameMode gameMode, DifficultyInfo.DifficultyEnum difficulty, 
                    string playlistName = null, bool isCustomPlaylist = true, string targetEnvName = null)
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

        _gameMode = gameMode;
        _difficulty = difficulty;
        _targetEnvName = targetEnvName;
        isValid = true;
    }

    public Playlist(PlaylistItem singleSong, string targetEnvName = null)
    {
        _playlistName = singleSong.SongName;
        _items = new[] {singleSong};
        _isCustomPlaylist = singleSong.IsCustomSong;
        _length = singleSong.SongInfo.SongLength;
        _gameMode = GameMode.Unset;
        _difficulty = DifficultyInfo.DifficultyEnum.Unset;
        _targetEnvName = targetEnvName;
        isValid = true;
    }
    
    public Playlist(Playlist sourcePlaylist, string targetEnvName)
    {
        _playlistName = sourcePlaylist.PlaylistName;
        _items = sourcePlaylist.Items;
        _isCustomPlaylist = sourcePlaylist.IsCustomPlaylist;
        _length = sourcePlaylist.Length;
        _gameMode = sourcePlaylist.GameModeOverride;
        _difficulty = sourcePlaylist.DifficultyEnum;
        _targetEnvName = targetEnvName;
        isValid = true;
    }

    public Playlist(Playlist sourcePlaylist, DifficultyInfo.DifficultyEnum difficultyEnum)
    {
        _playlistName = sourcePlaylist.PlaylistName;
        _items = sourcePlaylist.Items;
        _isCustomPlaylist = sourcePlaylist.IsCustomPlaylist;
        _length = sourcePlaylist.Length;
        _gameMode = sourcePlaylist.GameModeOverride;
        _difficulty = difficultyEnum;
        _targetEnvName = sourcePlaylist.TargetEnvName;
        isValid = true;
    }
    
    public Playlist(Playlist sourcePlaylist, GameMode gameMode)
    {
        _playlistName = sourcePlaylist.PlaylistName;
        _items = sourcePlaylist.Items;
        _isCustomPlaylist = sourcePlaylist.IsCustomPlaylist;
        _length = sourcePlaylist.Length;
        _gameMode = gameMode;
        _difficulty = sourcePlaylist.DifficultyEnum;
        _targetEnvName = sourcePlaylist.TargetEnvName;
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

    public void SetGameMode(GameMode mode)
    {
        _gameMode = mode;
    }

    public void SetDifficulty(DifficultyInfo.DifficultyEnum difficultyEnum)
    {
        _difficulty = difficultyEnum;
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