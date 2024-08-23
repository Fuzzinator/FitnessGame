using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using GameModeManagement;
using UnityEngine;

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


    [field: SerializeField]
    public EnvAssetReference Gloves { get; private set; }

    [field: SerializeField]
    public EnvAssetReference Targets { get; private set; }

    [field: SerializeField]
    public EnvAssetReference Obstacles { get; private set; }


    [SerializeField]
    private ColorsManager.ColorSet _targetColors;

    [SerializeField]
    private bool _setStartingSide = false;

    [SerializeField]
    private HitSideType _startingSide = HitSideType.Right;

    public bool isValid;

    [SerializeField]
    private float _targetSpeedMod = 1f;

    [SerializeField]
    private string _version;

    [SerializeField]
    private string _guid;

    public bool IsCustomPlaylist => _isCustomPlaylist;
    public float Length => _length;
    public string PlaylistName => _playlistName;

    public string TargetEnvName => _targetEnvName;

    public string TargetEnvGlovesName
    {
        get
        {
            if (Gloves != null && !string.IsNullOrWhiteSpace(Gloves.AssetName))
            {
                return Gloves.AssetName;
            }
            else
            {
                return null;
            }
        }
    }
    public string TargetEnvTargetsName
    {
        get
        {
            if (Targets != null && !string.IsNullOrWhiteSpace(Targets.AssetName))
            {
                return Targets.AssetName;
            }
            else
            {
                return null;
            }
        }
    }
    public string TargetEnvObstaclesName
    {
        get
        {
            if (Obstacles != null && !string.IsNullOrWhiteSpace(Obstacles.AssetName))
            {
                return Obstacles.AssetName;
            }
            else
            {
                return null;
            }
        }
    }

    public GameMode TargetGameMode => _gameMode;

    public DifficultyInfo.DifficultyEnum DifficultyEnum => _difficulty;

    public ColorsManager.ColorSet TargetColors => _targetColors;

    public HitSideType StartingSide => _setStartingSide ? _startingSide : HitSideType.Left;
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

    [field: SerializeField]
    public bool ForceNoObstacles { get; private set; }
    [field: SerializeField]
    public bool ForceOneHanded { get; private set; }
    [field: SerializeField]
    public bool ForceJabsOnly { get; private set; }

    public float TargetSpeedMod => _targetSpeedMod != 0 ? _targetSpeedMod : 1;

    public string Version => _version;

    public string GUID => _guid;

    private const int MINUTE = 60;
    private const string DIVIDER = ":";
    private const string PLAYLISTVERSION = "0.0.5";

    public Playlist(List<PlaylistItem> items, GameMode gameMode, DifficultyInfo.DifficultyEnum difficulty, HitSideType startingSide, float targetSpeedMod,
        string playlistName = null, bool isCustomPlaylist = true, string targetEnvName = null, Texture2D image = null,
        EnvAssetReference gloves = null, EnvAssetReference targets = null, EnvAssetReference obstacles = null,
        bool forceNoObstacles = false, bool forceOneHanded = false, bool forceJabsOnly = false)
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
        _targetSpeedMod = targetSpeedMod;
        _version = PLAYLISTVERSION;
        _guid = Guid.NewGuid().ToString();
        _startingSide = startingSide;
        _setStartingSide = true;
        if (gloves != null)
        {
            Gloves = gloves;
        }
        else
        {
            Gloves = EnvironmentControlManager.Instance.GetGloveAtIndex(0);
        }

        if (targets != null)
        {
            Targets = targets;
        }
        else
        {
            Targets = EnvironmentControlManager.Instance.GetTargetAtIndex(0);
        }

        if (obstacles != null)
        {
            Obstacles = obstacles;
        }
        else
        {
            Obstacles = EnvironmentControlManager.Instance.GetObstacleAtIndex(0);
        }

        isValid = true;
        ForceNoObstacles = forceNoObstacles;
        ForceOneHanded = forceOneHanded;
        ForceJabsOnly = forceJabsOnly;
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
        sourcePlaylist.Items.CopyTo(_items, 0);

        _isCustomPlaylist = sourcePlaylist.IsCustomPlaylist;
        _length = sourcePlaylist.Length;
        _gameMode = sourcePlaylist.TargetGameMode;
        _difficulty = sourcePlaylist.DifficultyEnum;
        _targetEnvName = sourcePlaylist.TargetEnvName;
        _targetColors = sourcePlaylist.TargetColors;
        _image = sourcePlaylist.PlaylistImage;
        _startingSide = sourcePlaylist.StartingSide;
        _targetSpeedMod = sourcePlaylist.TargetSpeedMod;
        _version = sourcePlaylist.Version;
        _guid = sourcePlaylist.GUID;
        Gloves = sourcePlaylist.Gloves;
        Targets = sourcePlaylist.Targets;
        Obstacles = sourcePlaylist.Obstacles;
        ForceNoObstacles = sourcePlaylist.ForceNoObstacles;
        ForceOneHanded = sourcePlaylist.ForceOneHanded;
        ForceJabsOnly = sourcePlaylist.ForceJabsOnly;
        isValid = true;
    }

    public Playlist(PlaylistItem singleSong, HitSideType startingSide, bool forceNoObstacles, bool forceOneHanded, bool forceJabsOnly, float targetSpeedMod, string targetEnvName = null, Sprite image = null)
    {
        _playlistName = singleSong.SongName;
        _items = new[] { singleSong };
        _isCustomPlaylist = singleSong.IsCustomSong;
        _length = singleSong.SongInfo.SongLength;
        _gameMode = GameMode.Unset;
        _difficulty = DifficultyInfo.DifficultyEnum.Unset;
        _targetEnvName = targetEnvName;
        _targetColors = ColorsManager.Instance.ActiveColorSet;
        _startingSide = startingSide;
        _setStartingSide = true;
        _image = image;
        _targetSpeedMod = targetSpeedMod;
        _version = PLAYLISTVERSION;
        ForceNoObstacles |= forceNoObstacles;
        ForceOneHanded = forceOneHanded;
        ForceJabsOnly = forceJabsOnly;
        _guid = null;
        isValid = true;
    }

    public void SetIcon(Texture2D texture)
    {
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
            Vector2.one * .5f, 100f, 0, SpriteMeshType.FullRect);
        _image = sprite;
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

    public void SetEnvironment(string envName)
    {
        _targetEnvName = envName;
    }

    public void SetColorSet(ColorsManager.ColorSet set)
    {
        _targetColors = set;
    }

    public void SetForwardFoot(HitSideType type)
    {
        _setStartingSide = true;
        _startingSide = type;
    }

    public void SetForceNoObstacles(bool on)
    {
        ForceNoObstacles = on;
    }

    public void SetForceOneHanded(bool on)
    {
        ForceOneHanded = on;
    }

    public void SetForceJabsOnly(bool on)
    {
        ForceJabsOnly = on;
    }

    public void SetTargetSpeedMod(float speedMod)
    {
        _targetSpeedMod = speedMod;
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

        return new PlaylistItem(item.SongInfo, targetDifficulty.Readable(), targetDifficulty, targetMode, item.ForceNoObstacles, item.ForceOneHanded, item.ForceJabsOnly);
    }

    #endregion
}