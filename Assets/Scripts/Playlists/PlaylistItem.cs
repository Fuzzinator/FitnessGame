using System;
using System.Collections;
using System.Collections.Generic;
using GameModeManagement;
using UnityEngine;

[Serializable]
public struct PlaylistItem
{
    [SerializeField]
    private string _songName;

    public string SongName => _songName;

    [SerializeField]
    private string _fileLocation;

    public string FileLocation => _fileLocation;

    [SerializeField]
    private string _difficulty;

    [SerializeField]
    private bool _isCustomSong;

    public bool IsCustomSong => _isCustomSong;

    [SerializeField]
    private GameMode _gameMode;

    public GameMode TargetGameMode
    {
        get => _gameMode;
        private set => _gameMode = value;
    }

    [SerializeField]
    private SongInfo _songInfo;

    public SongInfo SongInfo
    {
        get => _songInfo;
        set => _songInfo = value;
    }

    public PlaylistItem(string songName, string fileLocation, string difficulty, bool isCustomSong, GameMode gameMode,
        SongInfo info)
    {
        _songName = songName;
        _fileLocation = fileLocation;
        _difficulty = difficulty;
        _isCustomSong = isCustomSong;
        _gameMode = gameMode;
        _songInfo = info;
    }

    public string Difficulty => _difficulty;

    private static bool StringMatches(string string1, string string2)
    {
        if (string.IsNullOrWhiteSpace(string1) && string.IsNullOrWhiteSpace(string2))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(string1) || string.IsNullOrWhiteSpace(string2))
        {
            return false;
        }

        return string1.Equals(string2, StringComparison.InvariantCulture);
    }

    public static bool operator ==(PlaylistItem item1, PlaylistItem item2)
    {
        return StringMatches(item1.SongName, item2.SongName) &&
               StringMatches(item1.FileLocation, item2.FileLocation) &&
               StringMatches(item1.Difficulty, item2.Difficulty);
    }

    public static bool operator !=(PlaylistItem item1, PlaylistItem item2)
    {
        return !(item1 == item2);
    }

    public bool Equals(PlaylistItem other)
    {
        return _songName == other._songName && _fileLocation == other._fileLocation && _difficulty == other._difficulty;
    }

    public override bool Equals(object obj)
    {
        return obj is PlaylistItem other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (_songName != null ? _songName.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (_fileLocation != null ? _fileLocation.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (_difficulty != null ? _difficulty.GetHashCode() : 0);
            return hashCode;
        }
    }
}