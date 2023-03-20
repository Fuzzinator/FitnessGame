using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using GameModeManagement;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

[Serializable]
public class Playlist
{
    [SerializeField]
    private string _playlistName;

    //workout length in seconds
    [SerializeField]
    private float _length;

    [SerializeField]
    private Sprite _image;

    public Sprite PlaylistImage => _image;

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

    [SerializeField]
    private ColorsManager.ColorSet _targetColors;

    public bool isValid;

    [SerializeField]
    private string _version;

    [SerializeField]
    private string _guid;
    
    public bool IsCustomPlaylist => _isCustomPlaylist;
    public float Length => _length;
    public string PlaylistName => _playlistName;

    public string TargetEnvName => _targetEnvName;

    public GameMode TargetGameMode => _gameMode;

    public DifficultyInfo.DifficultyEnum DifficultyEnum => _difficulty;

    public ColorsManager.ColorSet TargetColors => _targetColors;

    public string ReadableLength
    {
        get
        {
            var minutes = (int) Mathf.Floor(_length / MINUTE);
            var seconds = (int) Mathf.Floor(_length % MINUTE);
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

    public string Version => _version;

    public string GUID => _guid;

    private const int MINUTE = 60;
    private const string DIVIDER = ":";
    private const string PLAYLISTVERSION = "0.0.2";

    public Playlist(List<PlaylistItem> items, GameMode gameMode, DifficultyInfo.DifficultyEnum difficulty,
        string playlistName = null, bool isCustomPlaylist = true, string targetEnvName = null, Texture2D image = null)
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
        _targetColors = ColorsManager.Instance.ActiveColorSet;
        SetIcon(image);
        _version = PLAYLISTVERSION;
        _guid = Guid.NewGuid().ToString();
        isValid = true;
    }

    public Playlist(Playlist sourcePlaylist, string targetEnvName)
    {
        CopyPlaylist(sourcePlaylist);
        _targetEnvName = targetEnvName;
    }
    
    public Playlist(Playlist sourcePlaylist, GameMode gameMode)
    {
        CopyPlaylist(sourcePlaylist);
        _gameMode = gameMode;
    }
    
    public Playlist(Playlist sourcePlaylist, DifficultyInfo.DifficultyEnum difficulty)
    {
        CopyPlaylist(sourcePlaylist);
        _difficulty = difficulty;
    }

    public void CopyPlaylist(Playlist sourcePlaylist)
    {
        _playlistName = sourcePlaylist.PlaylistName;
        _items = new PlaylistItem[sourcePlaylist.Items.Length];
        sourcePlaylist.Items.CopyTo(_items,0);
        
        _isCustomPlaylist = sourcePlaylist.IsCustomPlaylist;
        _length = sourcePlaylist.Length;
        _gameMode = sourcePlaylist.TargetGameMode;
        _difficulty = sourcePlaylist.DifficultyEnum;
        _targetEnvName = sourcePlaylist.TargetEnvName;
        _targetColors = sourcePlaylist.TargetColors;
        _image = sourcePlaylist.PlaylistImage;
        _version = sourcePlaylist.Version;
        _guid = sourcePlaylist.GUID;
        isValid = true;
    }
    
    public Playlist(PlaylistItem singleSong, string targetEnvName = null, Sprite image = null)
    {
        _playlistName = singleSong.SongName;
        _items = new[] {singleSong};
        _isCustomPlaylist = singleSong.IsCustomSong;
        _length = singleSong.SongInfo.SongLength;
        _gameMode = GameMode.Unset;
        _difficulty = DifficultyInfo.DifficultyEnum.Unset;
        _targetEnvName = targetEnvName;
        _targetColors = ColorsManager.Instance.ActiveColorSet;
        _image = image;
        _version = PLAYLISTVERSION;
        _guid = null;
        isValid = true;
    }

    public void SetIcon(Texture2D texture)
    {
        var sprite = Sprite.Create(texture, new Rect(0,0, texture.width, texture.height),
            Vector2.one *.5f, 100f);
        _image = sprite;
    }
    
    /*public Playlist(Playlist sourcePlaylist, string targetEnvName)
    {
        _playlistName = sourcePlaylist.PlaylistName;
        _items = sourcePlaylist.Items;
        _isCustomPlaylist = sourcePlaylist.IsCustomPlaylist;
        _length = sourcePlaylist.Length;
        _gameMode = sourcePlaylist.GameModeOverride;
        _difficulty = sourcePlaylist.DifficultyEnum;
        _targetEnvName = targetEnvName;
        _targetColors = ColorsManager.Instance.ActiveColorSet;
        _image = sourcePlaylist.PlaylistImage;
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
        _targetColors = ColorsManager.Instance.ActiveColorSet;
        _image = sourcePlaylist.PlaylistImage;
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
        _targetColors = ColorsManager.Instance.ActiveColorSet;
        _image = sourcePlaylist.PlaylistImage;
    }*/

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
    
    public void SetEnvironment(string envName)
    {
        _targetEnvName = envName;
    }

    public void SetColorSet(ColorsManager.ColorSet set)
    {
        _targetColors = set;
    }

    public enum SortingMethod
    {
        None = 0,
        PlaylistName = 1,
        InversePlaylistName = 2,
        PlaylistLength = 3,
        InversePlaylistLength = 4
    }

    #region Upgrade Support

    public void UpgradePlaylistSoSongsAreOverrides()
    {
        _version = PLAYLISTVERSION;
        for (var i = 0; i < _items.Length; i++)
        {
            _items[i] = UpdatePlaylistItemNormalToUnset(_items[i]);
        }
    }

    public void UpgradePlaylistAddGuid()
    {
        _guid = Guid.NewGuid().ToString();
    }

    private PlaylistItem UpdatePlaylistItemNormalToUnset(PlaylistItem item)
    {
        var targetDifficulty = item.DifficultyEnum;
        if (item.DifficultyEnum == DifficultyInfo.DifficultyEnum.Normal)
        {
            targetDifficulty = DifficultyInfo.DifficultyEnum.Unset;
        }

        var targetMode = item.TargetGameMode;
        if (item.TargetGameMode == GameMode.Normal)
        {
            targetMode = GameMode.Unset;
        }

        return new PlaylistItem(item.SongInfo, targetDifficulty.Readable(), targetDifficulty, targetMode);
    }

    #endregion
}