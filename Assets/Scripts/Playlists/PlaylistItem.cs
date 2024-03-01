using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
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
    private DifficultyInfo.DifficultyEnum _difficultyEnum;

    [SerializeField]
    private bool _isCustomSong;

    [field: SerializeField]
    public string SongID { get; private set; }

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

    [field: SerializeField]
    public bool ForceNoObstacles { get; private set; }
    [field: SerializeField]
    public bool ForceOneHanded { get; private set; }
    [field: SerializeField]
    public bool ForceJabsOnly { get; private set; }

    public SongInfo SongInfo
    {
        get => _songInfo;
        set => _songInfo = value;
    }

    public PlaylistItem(SongInfo songInfo, string difficulty, DifficultyInfo.DifficultyEnum difficultyEnum, GameMode gameMode, bool forceNoObstacles, bool forceOnHanded, bool forceJabsOnly)
    {
        _songName = songInfo.SongName;
        _fileLocation = songInfo.fileLocation;
        _difficulty = difficulty;
        _isCustomSong = songInfo.isCustomSong;
        _gameMode = gameMode;
        _songInfo = songInfo;
        _difficultyEnum = difficultyEnum;
        SongID = songInfo.SongID;
        ForceNoObstacles = forceNoObstacles;
        ForceOneHanded = forceOnHanded;
        ForceJabsOnly = forceJabsOnly;
    }

    public string Difficulty => _difficulty;
    public DifficultyInfo.DifficultyEnum DifficultyEnum => _difficultyEnum;

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
        if (!string.IsNullOrWhiteSpace(item1.SongID) && !string.IsNullOrWhiteSpace(item2.SongID))
        {
            return string.Equals(item1.SongID, item2.SongID);
        }

        return StringMatches(item1.SongName, item2.SongName) &&
               StringMatches(item1.FileLocation, item2.FileLocation) &&
               StringMatches(item1.Difficulty, item2.Difficulty) &&
               item1.TargetGameMode == item2.TargetGameMode;
    }

    public static bool operator !=(PlaylistItem item1, PlaylistItem item2)
    {
        return !(item1 == item2);
    }

    public bool Equals(PlaylistItem other)
    {
        return _fileLocation == other._fileLocation && _difficulty == other._difficulty && TargetGameMode == other.TargetGameMode;
    }

    public override bool Equals(object obj)
    {
        return obj is PlaylistItem other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_songName, _fileLocation, _difficulty, TargetGameMode);
    }
}