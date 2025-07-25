using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameModeManagement;
using InfoSaving;
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

    [SerializeField]
    private bool _overrideDifficulties;

    [SerializeField]
    private bool _overrideGameModes;

    public Playlist CurrentPlaylist
    {
        get => _currentPlaylist;
        set
        {
            _currentPlaylist = value;
            currentPlaylistUpdated?.Invoke(value);
            _activePlaylistIsTemp = false;
        }
    }

    public UnityEvent<PlaylistItem> playlistItemUpdated = new UnityEvent<PlaylistItem>();
    public UnityEvent<Playlist> currentPlaylistUpdated = new UnityEvent<Playlist>();

    private int _currentIndex = 0;

    private float _timePassed;

    private bool _activePlaylistIsTemp = false;

    public bool ActivePlaylistIsTemp => _activePlaylistIsTemp;

    public int CurrentIndex => _currentIndex;

    public int SongCount => _currentPlaylist?.Items?.Length ?? 0;

    public bool OverrideDifficulties => _overrideDifficulties;

    public bool OverrideGameModes => _overrideGameModes;

    public float RemainingTime => _currentPlaylist.Length - _timePassed;

    private const string DefaultForceNoObstaclesSetting = "DefaultAllowObstacles";
    private const string DefaultForceOneHandedSetting = "DefaultForceOneHanded";
    private const string DefaultForceJabsOnlySetting = "DefaultForceJabsOnly";
    private const string DefaultTargetSpeedModSetting = "DefaultTargetSpeedMod-";
    private const string DefaultSongSpeedModSetting = "DefaultSongSpeedMod-";

    public GameMode TargetGameMode
    {
        get
        {
            if (!_overrideGameModes && _currentItem.TargetGameMode != GameMode.Unset)
            {
                return _currentItem.TargetGameMode;
            }

            if (_currentPlaylist != null && _currentPlaylist.TargetGameMode != GameMode.Unset)
            {
                return _currentPlaylist.TargetGameMode;
            }

            return GameMode.Normal;
        }
    }

    public DifficultyInfo.DifficultyEnum TargetDifficulty
    {
        get
        {
            if (!_overrideDifficulties && _currentItem.DifficultyEnum != DifficultyInfo.DifficultyEnum.Unset)
            {
                return _currentItem.DifficultyEnum;
            }

            if (_currentPlaylist != null && _currentPlaylist.DifficultyEnum != DifficultyInfo.DifficultyEnum.Unset)
            {
                return _currentPlaylist.DifficultyEnum;
            }

            return DifficultyInfo.DifficultyEnum.Normal;
        }
    }

    public bool ForceOneHanded
    {
        get
        {
            return _currentPlaylist.ForceOneHanded || _currentItem.ForceOneHanded;
        }
    }

    public bool ForceNoObstacles
    {
        get
        {
            return _currentPlaylist.ForceNoObstacles || _currentItem.ForceNoObstacles;
        }
    }

    public bool ForceJabsOnly
    {
        get
        {
            return _currentPlaylist.ForceJabsOnly || _currentItem.ForceJabsOnly || TargetGameMode == GameMode.JabsOnly;
        }
    }

    public float TargetSpeedMod => _currentPlaylist.TargetSpeedMod;

    public float SongSpeedMod => _currentPlaylist.SongSpeedMod;

    public float CurrentSongLength => CurrentItem.SongInfo.SongLength / _currentPlaylist.SongSpeedMod;


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
        _timePassed = 0f;
        _currentIndex = 0;
        if (_currentPlaylist?.Items == null || _currentPlaylist.Items.Length <= _currentIndex)
        {
            Debug.LogError("Playlist has no items");
            return;
        }

        CurrentItem = _currentPlaylist.Items[_currentIndex];
    }

    public void UpdateCurrentPlaylist()
    {
        _timePassed += CurrentSongLength;

        _currentIndex++;

        if (_currentPlaylist?.Items == null || _currentPlaylist.Items.Length == 0 ||
            _currentIndex >= _currentPlaylist.Items.Length)
        {
            return;
        }

        CurrentItem = _currentPlaylist.Items[_currentIndex];
    }

    public void RestartSong()
    {
        _timePassed -= CurrentSongLength;
        _currentIndex--;
    }

    public void Restart()
    {
        _currentIndex = -1;
    }

    public void FullReset()
    {
        _currentIndex = 0;
        _currentItem = new PlaylistItem();
        //_currentPlaylist = null;
        SetOverrideDifficulty(false);
        SetOverrideGameMode(false);
    }

    public void SetTempSongPlaylist(PlaylistItem playlistItem, HitSideType forwardFootSide, 
        bool noObstacles, bool oneHanded, bool jabsOnly, float targetSpeedMod, float songSpeedMod)
    {
        var targetEnvName = EnvironmentControlManager.Instance.GetTargetEnvironment().Name;
        var tempPlaylist = new Playlist(playlistItem, forwardFootSide, noObstacles, oneHanded, jabsOnly, targetSpeedMod, songSpeedMod, targetEnvName);
        CurrentPlaylist = tempPlaylist;
        _activePlaylistIsTemp = true;
    }

    public void SetEnvironment(string envName)
    {
        _currentPlaylist = new Playlist(_currentPlaylist, envName);
    }

    public void SetOverrideDifficulty(bool overrideDifficulty)
    {
        _overrideDifficulties = overrideDifficulty;
    }


    public void SetOverrideGameMode(bool overrideGameMode)
    {
        _overrideGameModes = overrideGameMode;
    }

    public void SetDifficulty(DifficultyInfo.DifficultyEnum difficultyEnum)
    {
        _currentPlaylist = new Playlist(_currentPlaylist, difficultyEnum);
    }

    public void SetGameMode(GameMode gameMode)
    {
        _currentPlaylist = new Playlist(_currentPlaylist, gameMode);
    }

    public string GetFullSongName(SongInfo info = null, string prefix = null, string suffix = null, bool noID = false)
    {
        if (info == null)
        {
            info = SongInfoReader.Instance?.songInfo;
        }

        if (info == null)
        {
            return string.Empty;
        }
        if (noID)
        {
            return SongInfoReader.GetFullSongNameNoID(info, TargetDifficulty, TargetGameMode, prefix, suffix);
        }
        else
        {
            return SongInfoReader.GetFullSongName(info, TargetDifficulty, TargetGameMode, prefix, suffix);
        }
    }

    public string GetOnlineRecordName(SongInfo info = null)
    {
        if (info == null)
        {
            info = SongInfoReader.Instance?.songInfo;
        }
        var recordableName = info.RecordableName;
        if (string.Equals(recordableName, "LOCAL"))
        {
            return recordableName;
        }

        var songID = $"Song_{recordableName}{TargetDifficulty}{TargetGameMode}";
        return songID;
    }

    
    public async UniTask<SongRecord[]> TryGetRecords(CancellationToken token)
    {
        return await PlayerStatsFileManager.TryGetRecords(SongInfoReader.Instance.songInfo, TargetDifficulty, TargetGameMode, token);
    }


    public static void SetDefaultForceNoObstacles(bool isOn)
    {
        SettingsManager.SetSetting(DefaultForceNoObstaclesSetting, isOn);
    }

    public static void SetDefaultForceOneHanded(bool isOn)
    {
        SettingsManager.SetSetting(DefaultForceOneHandedSetting, isOn);
    }

    public static void SetDefaultForceJabsOnly(bool isOn)
    {
        SettingsManager.SetSetting(DefaultForceJabsOnlySetting, isOn);
    }

    public static void SetDefaultTargetSpeedMod(float targetSpeedMod)
    {
        SettingsManager.SetSetting(DefaultTargetSpeedModSetting, targetSpeedMod);
    }

    public static void SetDefaultSongSpeedMod(float targetSpeedMod)
    {
        SettingsManager.SetSetting(DefaultSongSpeedModSetting, targetSpeedMod);
    }

    public static bool GetDefaultForceNoObstacles()
    {
        return SettingsManager.GetSetting(DefaultForceNoObstaclesSetting, false);
    }

    public static bool GetDefaultForceOneHanded()
    {
        return SettingsManager.GetSetting(DefaultForceOneHandedSetting, false);
    }

    public static bool GetDefaultForceJabsOnly()
    {
        return SettingsManager.GetSetting(DefaultForceJabsOnlySetting, false);
    }

    public static float GetDefaultTargetSpeedMod()
    {
        return SettingsManager.GetSetting(DefaultTargetSpeedModSetting, 1f);
    }

    public static float GetDefaultSongSpeedMod()
    {
        return SettingsManager.GetSetting(DefaultSongSpeedModSetting, 1f);
    }
}