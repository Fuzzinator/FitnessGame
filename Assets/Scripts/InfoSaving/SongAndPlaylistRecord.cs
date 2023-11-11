using System;
using UnityEngine;

public struct SongAndPlaylistRecords
{
    public bool hasRecord;
    public SongAndPlaylistScoreRecord[] scores;
    public SongAndPlaylistStreakRecord[] streaks;

    public SongAndPlaylistRecords(bool hasRecord, SongAndPlaylistScoreRecord[] scores, SongAndPlaylistStreakRecord[] streaks)
    {
        this.hasRecord = hasRecord;
        this.scores = scores;
        this.streaks = streaks;
    }
}

[Serializable]
public struct SongRecord
{
    [SerializeField]
    private string _profileName;
    [SerializeField]
    private string _guid;
    [SerializeField]
    private int _score;
    [SerializeField]
    private int _streak;
    [SerializeField]
    private bool _isValid;

    public string ProfileName => _profileName;
    public string GUID => _guid;
    public int Score => _score;
    public int Streak => _streak;

    public bool IsValid => _isValid;

    public SongRecord(string profileName, string guid, int score, int streak)
    {
        _profileName = profileName;
        _guid = string.IsNullOrWhiteSpace(guid) ? Guid.NewGuid().ToString() : guid;
        _score = score;
        _streak = streak;
        _isValid = true;
    }
}

[Serializable]
public struct SongAndPlaylistScoreRecord
{
    [SerializeField]
    private ulong _score;

    [SerializeField]
    private string _profileName;

    [SerializeField]
    private string _profileGUID;

    [SerializeField]
    private bool _isValid;
    public ulong Score => _score;
    public string ProfileName => _profileName;
    public bool IsValid => _isValid;

    public SongAndPlaylistScoreRecord(ulong score, string profileName = null, string profileGuid = null)
    {
        _score = score;
        _profileName = profileName;
        _profileGUID = profileGuid;
        _isValid = true;
    }
}

[Serializable]
public struct SongAndPlaylistStreakRecord
{
    [SerializeField]
    private int _streak;

    [SerializeField]
    private string _profileName;

    [SerializeField]
    private string _profileGUID;

    [SerializeField]
    private bool _isValid;
    public int Streak => _streak;
    public string ProfileName => _profileName;
    public bool IsValid => _isValid;

    public SongAndPlaylistStreakRecord(int streak, string profileName = null, string profileGuid = null)
    {
        _streak = streak;
        _profileName = profileName;
        _profileGUID = profileGuid;
        _isValid = true;
    }
}
