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

    private bool _activePlaylistIsTemp = false;

    public bool ActivePlaylistIsTemp => _activePlaylistIsTemp;

    public int CurrentIndex => _currentIndex;

    public int SongCount => _currentPlaylist?.Items?.Length ?? 0;

    public bool OverrideDifficulties => _overrideDifficulties;

    public bool OverrideGameModes => _overrideGameModes;

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
        _currentIndex++;

        if (_currentPlaylist?.Items == null || _currentPlaylist.Items.Length == 0 ||
            _currentIndex >= _currentPlaylist.Items.Length)
        {
            return;
        }

        CurrentItem = _currentPlaylist.Items[_currentIndex];
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

    public void SetTempSongPlaylist(PlaylistItem playlistItem, HitSideType forwardFootSide)
    {
        var targetEnvName = EnvironmentControlManager.Instance.GetTargetEnvName();
        var tempPlaylist = new Playlist(playlistItem, forwardFootSide, targetEnvName);
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

    public string GetFullSongName(SongInfo info = null, string prefix = null, string suffix = null)
    {
        if (info == null)
        {
            info = SongInfoReader.Instance?.songInfo;
        }

        if (info == null)
        {
            return string.Empty;
        }
        
        return SongInfoReader.GetFullSongName(info,TargetDifficulty, TargetGameMode, prefix, suffix);
    }

    public async UniTask<SongAndPlaylistRecords> TryGetRecords(CancellationToken token)
    {
        return await PlayerStatsFileManager.TryGetRecords(SongInfoReader.Instance.songInfo, TargetDifficulty, TargetGameMode, token);
    }
}