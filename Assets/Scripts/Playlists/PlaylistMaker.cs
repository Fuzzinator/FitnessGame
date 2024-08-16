using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using GameModeManagement;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;
using static BeatsaberV3Choreography;

public class PlaylistMaker : MonoBehaviour, IProgress<float>
{
    public static PlaylistMaker Instance { get; private set; }

    private List<PlaylistItem> _playlistItems = new List<PlaylistItem>();
    private SongInfo _activeItem;
    private int _activeDataIndex;
    public SongInfo DisplayedSongInfo => _activeItem;

    [SerializeField]
    private UnityEvent _playlistItemsUpdated = new UnityEvent();

    [SerializeField]
    private UnityEvent _startWritingPlaylist = new UnityEvent();

    [SerializeField]
    private UnityEvent<Playlist> _newPlaylistCreated = new UnityEvent<Playlist>();

    [SerializeField]
    private GameMode _gameMode = GameMode.Unset;

    [SerializeField]
    private DifficultyInfo.DifficultyEnum _difficulty = DifficultyInfo.DifficultyEnum.Unset;

    [SerializeField]
    private float _targetSpeedMod = 1f;

    [SerializeField]
    private Texture2D _emptyTexture;

    public List<PlaylistItem> PlaylistItems => _playlistItems;

    private bool _editMode = false;
    private HitSideType _startingSide = HitSideType.Unused;

    public string PlaylistName => _playlistName;
    public GameMode TargetGameMode => _gameMode;
    public DifficultyInfo.DifficultyEnum Difficulty => _difficulty;

    public float TargetSpeedMod => _targetSpeedMod;

    public HitSideType StartingSide

    {
        get
        {
            if(_startingSide == HitSideType.Unused)
            {
                var leftHanded = SettingsManager.GetSetting(LeftHanded, false);
                var defaultFooting = SettingsManager.GetSetting(DefaultFootSetting, leftHanded ? 0 : 1);
                _startingSide = (HitSideType)defaultFooting;
            }
            return _startingSide;
        }
    }

    private string _playlistName;
    private string _originalName;
    private string _targetEnv;

    [field: SerializeField]
    public EnvAssetReference Gloves { get; private set; }

    [field: SerializeField]
    public EnvAssetReference Targets { get; private set; }

    [field: SerializeField]
    public EnvAssetReference Obstacles { get; private set; }

    public string GlovesName => Gloves?.AssetName;

    public string TargetsName => Targets?.AssetName;

    public string ObstaclesName => Obstacles?.AssetName;

    public bool ForceNoObstacles => _forceNoObstacles;

    public bool ForceOneHanded => _forceOneHanded;

    public bool ForceJabsOnly => _forceJabsOnly;

    private bool _changesMade = false;

    private CancellationToken _cancallationToken;

    public UnityEvent<int> TargetEnvironmentIndexChanged { get; private set; } = new UnityEvent<int>();

    private bool _forceNoObstacles;
    private bool _forceOneHanded;
    private bool _forceJabsOnly;

    #region Const Strings

    private const string NEWPLAYLISTNAME = "New Playlist";
    private const string PLAYLISTEXTENSION = ".txt";

    private const int MINUTE = 60;
    private const string DIVIDER = ":";

    private const string LeftHanded = "LeftHanded";
    private const string DefaultFootSetting = "DefaultForwardFoot";
    #endregion

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

    private void Start()
    {
        _cancallationToken = this.GetCancellationTokenOnDestroy();
        RefreshOverrides();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetActiveItem(SongInfo info)
    {
        _activeItem = info;
    }

    public void SetActivePlaylistItem(int dataIndex)
    {
        _activeDataIndex = dataIndex;
    }

    public static PlaylistItem GetPlaylistItem(SongInfo songInfo, string difficulty,
        DifficultyInfo.DifficultyEnum difficultyEnum, GameMode gameMode)
    {
        return new PlaylistItem(songInfo, difficulty, difficultyEnum, gameMode, Instance.ForceNoObstacles, Instance.ForceOneHanded, Instance.ForceJabsOnly);
    }
    public static PlaylistItem GetPlaylistItem(SongInfo songInfo, string difficulty,
        DifficultyInfo.DifficultyEnum difficultyEnum, GameMode gameMode,
        bool forceNoObstacles, bool forceOneHanded, bool forceJabsOnly)
    {
        return new PlaylistItem(songInfo, difficulty, difficultyEnum, gameMode, forceNoObstacles, forceOneHanded, forceJabsOnly);
    }

    public void AddPlaylistItem(PlaylistItem item)
    {
        if (GameManager.Instance.DemoMode && _playlistItems.Count >= GameManager.DemoModeMaxPlaylistLength)
        {
            var visuals = new Notification.NotificationVisuals(
                        $"Song cannot be added. The maximum number of songs in a custom playlist in this demo is {GameManager.DemoModeMaxPlaylistLength}. To create longer playlists, please consider buying the full game.",
                        "Demo Mode", autoTimeOutTime: 5f, button1Txt: "Okay");
            NotificationManager.RequestNotification(visuals);
        }
        else
        {
            _changesMade = true;
            _playlistItems.Add(item);
            _playlistItemsUpdated?.Invoke();
        }
    }

    public void UpdatePlaylistItem(PlaylistItem item)
    {
        if (_activeDataIndex < _playlistItems.Count && _activeDataIndex >= 0)
        {
            _changesMade = true;
            _playlistItems[_activeDataIndex] = item;
            _playlistItemsUpdated?.Invoke();
        }
    }

    public void RemovePlaylistItem(PlaylistItem item)
    {
        if (!_playlistItems.Contains(item))
        {
            Debug.LogWarning("Playlist not contained but trying to remove it. This shouldnt happen.");
            return;
        }
        _changesMade = true;
        _playlistItems.Remove(item);
        _playlistItemsUpdated?.Invoke();
    }
    public void RemoveActivePlaylistItem()
    {
        if (_activeDataIndex >= _playlistItems.Count && _activeDataIndex < 0)
        {
            Debug.LogWarning("Playlist not contained but trying to remove it. This shouldnt happen.");
            return;
        }
        _changesMade = true;
        _playlistItems.RemoveAt(_activeDataIndex);
        _playlistItemsUpdated?.Invoke();
    }

    public void AppendPlaylistItems(PlaylistItem item)
    {
        if (_playlistItems.Contains(item))
        {
            _playlistItems.Remove(item);
        }
        else
        {
            _playlistItems.Add(item);
        }

        _changesMade = true;
        _playlistItemsUpdated?.Invoke();
    }

    public void ShufflePlaylistItems()
    {
        _changesMade = true;
        _playlistItems.Shuffle();
    }

    public void SetPlaylistName(string newName)
    {
        _changesMade = true;
        _playlistName = newName;
    }

    public void SetTargetEnvironment(int envIndex)
    {
        var hasAsset = EnvironmentControlManager.Instance.TryGetEnvRefAtIndex(envIndex, out var envAsset);
        if (hasAsset && _targetEnv != envAsset.Name)
        {
            _changesMade = true;
            _targetEnv = envAsset.Name;
            TargetEnvironmentIndexChanged?.Invoke(envIndex);
        }
    }

    public void SetStartingType(HitSideType startingSide)
    {
        _changesMade = true;
        _startingSide = startingSide;
    }

    public void CreatePlaylist()
    {
        _changesMade = false;
        CreatePlaylistAsync().Forget();
    }

    private async UniTaskVoid CreatePlaylistAsync()
    {
        if (_playlistItems == null || _playlistItems.Count == 0)
        {
            //Debug.LogError("Cannot create empty playlist");
            var visuals = new Notification.NotificationVisuals("Cannot create empty playlist", "Failed to Save",
                autoTimeOutTime: 2.5f, popUp: true);
            NotificationManager.RequestNotification(visuals);
            return;
        }

        var sprite = await GetSprite();
        var newPlaylist = new Playlist(_playlistItems, _gameMode, _difficulty, _startingSide, TargetSpeedMod , _playlistName, true, _targetEnv, sprite, Gloves, Targets, Obstacles);
        PlaylistManager.Instance.CurrentPlaylist = newPlaylist;


        if (!Directory.Exists(AssetManager.PlaylistsPath))
        {
            Directory.CreateDirectory(AssetManager.PlaylistsPath);
        }

        _playlistName = _playlistName.RemoveIllegalIOCharacters();
        if (string.IsNullOrWhiteSpace(_playlistName))
        {
            _playlistName = newPlaylist.PlaylistName;
        }

        var filePath = $"{AssetManager.PlaylistsPath}{_playlistName}.txt";
        if (_editMode)
        {
            PlaylistFilesReader.Instance.RemovePlaylistByName(_originalName);
            if (_originalName != _playlistName && File.Exists($"{AssetManager.PlaylistsPath}{_originalName}.txt"))
            {
                AssetManager.DeletePlaylist(_originalName);
            }
        }

        if (!_editMode || (_editMode && _originalName != _playlistName))
        {
            var index = 0;
            while (File.Exists(filePath))
            {
                index++;
                filePath = $"{AssetManager.PlaylistsPath}{_playlistName}_{index:00}.txt";
                try
                {
                    await UniTask.DelayFrame(1, cancellationToken: _cancallationToken);
                }
                catch (Exception e) when (e is OperationCanceledException)
                {
                    break;
                }
            }

            if (index > 0)
            {
                _playlistName = $"{_playlistName}_{index:00}";
                newPlaylist.SetPlaylistName(_playlistName);
            }
        }

        var streamWriter = File.CreateText(filePath);
        var json = JsonUtility.ToJson(newPlaylist);
        var writingTask = streamWriter.WriteAsync(json);

        _startWritingPlaylist?.Invoke();

        await writingTask;

        streamWriter.Close();
        if (newPlaylist.PlaylistImage != null)
        {
            var bytes = newPlaylist.PlaylistImage.texture.EncodeToJPG();
            await File.WriteAllBytesAsync($"{AssetManager.PlaylistsPath}{_playlistName}.jpg", bytes, _cancallationToken);
        }

        _newPlaylistCreated?.Invoke(newPlaylist);
        _playlistItems.Clear();
        SetPlaylistName(NEWPLAYLISTNAME);
    }

    public async UniTask<Texture2D> GetSprite()
    {
        var texture1 = _playlistItems.Count > 0
            ? await _playlistItems[0].SongInfo.LoadTexture(_cancallationToken)
            : _emptyTexture;
        var texture2 = _playlistItems.Count > 1
            ? await _playlistItems[1].SongInfo.LoadTexture(_cancallationToken)
            : _emptyTexture;
        var texture3 = _playlistItems.Count > 2
            ? await _playlistItems[2].SongInfo.LoadTexture(_cancallationToken)
            : _emptyTexture;
        var texture4 = _playlistItems.Count > 3
            ? await _playlistItems[3].SongInfo.LoadTexture(_cancallationToken)
            : _emptyTexture;

        var combinedTexture = new Texture2D(512, 512, TextureFormat.RGB24, false);
        combinedTexture.SetPixels(0, 0, 256, 256, texture1.GetPixels(), 0);
        combinedTexture.SetPixels(256, 0, 256, 256, texture2.GetPixels(), 0);
        combinedTexture.SetPixels(0, 256, 256, 256, texture3.GetPixels(), 0);
        combinedTexture.SetPixels(256, 256, 256, 256, texture4.GetPixels(), 0);
        combinedTexture.Apply(false, false);

        if (combinedTexture == null)
        {
            Debug.LogError($"Created texture is null for some reason");
            return null;
        }

        return combinedTexture;
    }

    public float GetLength()
    {
        var length = 0f;
        foreach (var item in _playlistItems)
        {
            length += item.SongInfo.SongLength;
        }

        return length;
    }

    public string GetReadableLength()
    {
        var length = GetLength();

        var minutes = (int)Mathf.Floor(length / MINUTE);
        var seconds = (int)Mathf.Floor(length % MINUTE);
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

            return sb.ToString();
        }
    }

    public void SetEditMode(bool editMode)
    {
        var playlist = PlaylistManager.Instance.CurrentPlaylist;

        _editMode = editMode;
        if (_editMode)
        {
            _originalName = playlist.PlaylistName;
            _playlistName = playlist.PlaylistName;
            _difficulty = playlist.DifficultyEnum;
            _gameMode = playlist.TargetGameMode;
            _startingSide = playlist.StartingSide;

            _changesMade = false;
            _playlistItems.Clear();
            _playlistItems.AddRange(playlist.Items);

            _playlistItemsUpdated?.Invoke();
        }
        else
        {
            _originalName = null;
            _playlistName = string.Empty;

            _changesMade = false;
            _playlistItems.Clear();
            _playlistItemsUpdated?.Invoke();
            _gameMode = GameMode.Unset;
            _difficulty = DifficultyInfo.DifficultyEnum.Unset;
            _startingSide = HitSideType.Right;
        }
    }

    public void SetDifficulty(DifficultyInfo.DifficultyEnum difficultyEnum)
    {
        _changesMade = true;
        _difficulty = difficultyEnum;
    }

    public void SetGameMode(GameMode gameMode)
    {
        _changesMade = true;
        _gameMode = gameMode;
    }

    public void SetGloves(EnvAssetReference gloves)
    {
        if (Gloves == gloves || Gloves.AssetName == gloves.AssetName)
        {
            return;
        }

        _changesMade = true;
        Gloves = gloves;
    }

    public void SetTargets(EnvAssetReference targets)
    {
        if (Targets == targets || Targets.AssetName == targets.AssetName)
        {
            return;
        }

        _changesMade = true;
        Targets = targets;
    }

    public void SetObstacles(EnvAssetReference obstacles)
    {
        if (Obstacles == obstacles || Obstacles.AssetName == obstacles.AssetName)
        {
            return;
        }

        _changesMade = true;
        Obstacles = obstacles;
    }

    public void SetForceNoObstacles(bool on)
    {
        _forceNoObstacles = on;
    }

    public void SetForceOneHanded(bool on)
    {
        _forceOneHanded = on;
    }

    public void SetForceJabsOnly(bool on)
    {
        _forceJabsOnly = on;
    }

    public void SetTargetSpeedMod(float speed)
    {
        _targetSpeedMod = speed;
    }

    public void Report(float value)
    {
        Debug.Log(value);
    }

    public void TryReturnToHome()
    {
        if (_changesMade && _playlistItems != null && _playlistItems.Count > 0)
        {
            var visuals = new Notification.NotificationVisuals("Unsaved changes in playlist. Would you like to save?", "Unsaved Changes", "Yes", "No", "Cancel");
            NotificationManager.RequestNotification(visuals, () => CreatePlaylistAndGoHome(), () => MainMenuUIController.Instance.SetActivePage(0));
        }
        else
        {
            MainMenuUIController.Instance.SetActivePage(0);
        }
    }

    private void CreatePlaylistAndGoHome()
    {
        CreatePlaylist();
        MainMenuUIController.Instance.SetActivePage(0);
    }

    public void RefreshOverrides()
    {
        _forceNoObstacles = PlaylistManager.GetDefaultForceNoObstacles();
        _forceJabsOnly = PlaylistManager.GetDefaultForceJabsOnly();
        _forceOneHanded = PlaylistManager.GetDefaultForceOneHanded();
        _targetSpeedMod = PlaylistManager.GetDefaultTargetSpeedMod();
    }

    /*private async UniTask<Texture2D> CombineTextures(Texture2D texture1,Texture2D texture2,Texture2D texture3,Texture2D texture4)
    {
        
        var pixels1 = texture1.GetPixelData<Color32>(0);
        var pixels2 = texture1.GetPixelData<Color32>(0);
        var pixels3 = texture1.GetPixelData<Color32>(0);
        var pixels4 = texture1.GetPixelData<Color32>(0);
        
        var finalArray = finalTexture.GetPixelData<Color32>(0);/*new NativeArray<Color32>(pixels1.Length + pixels2.Length + pixels3.Length + pixels4.Length,
            Allocator.TempJob);#1#
        try
        {
            var combineJob = new CombineTexturesJob(finalArray, pixels1, pixels2, pixels3, pixels4);
            await combineJob.Schedule(finalArray.Length, 16);
            finalTexture.Apply();
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}\n{e.StackTrace}");
        }
        finally
        {
            pixels1.Dispose();
            pixels2.Dispose();
            pixels3.Dispose();
            pixels4.Dispose();
            finalArray.Dispose();
        }

        return finalTexture;
    }
    
    private struct CombineTexturesJob : IJobParallelFor
    {
        private NativeArray<Color32> _finalPixels;

        private readonly NativeArray<Color32> _pixels1;
        private readonly NativeArray<Color32> _pixels2;
        private readonly NativeArray<Color32> _pixels3;
        private readonly NativeArray<Color32> _pixels4;

        private const int WIDTH = 512;
        private const int HALFWIDTH = 256;

        public CombineTexturesJob(NativeArray<Color32> finalPixels, NativeArray<Color32> pixels1, NativeArray<Color32> pixels2,
            NativeArray<Color32> pixels3, NativeArray<Color32> pixels4)
        {
            _finalPixels = finalPixels;
            _pixels1 = pixels1;
            _pixels2 = pixels2;
            _pixels3 = pixels3;
            _pixels4 = pixels4;
        }
        
        public void Execute(int index)
        {
            var height = index % WIDTH;
            var width = index - height * WIDTH;


            var color = new Color32();
            if (width < HALFWIDTH)
            {
                if (height < HALFWIDTH)
                {
                    color = _pixels1[height * HALFWIDTH + width];
                }
                else
                {
                    height -= HALFWIDTH;
                    color = _pixels3[height * HALFWIDTH + width];
                }
            }
            else
            {
                width -= HALFWIDTH;
                if (height < HALFWIDTH)
                {
                    color = _pixels2[height * HALFWIDTH + width];
                }
                else
                {
                    height -= HALFWIDTH;
                    color = _pixels4[height * HALFWIDTH + width];
                }
            }

            _finalPixels[index] = color;
            return;
            
            
            
            if (index < _pixels1.Length)
            {
                _finalPixels[index] = _pixels1[index];
            }
            else if (index < _pixels1.Length+_pixels2.Length)
            {
                var i = index - _pixels1.Length;
                _finalPixels[index] = _pixels2[i];
            }
            else if (index < _pixels1.Length+_pixels2.Length+_pixels3.Length)
            {
                var i = index - _pixels1.Length - _pixels2.Length;
                _finalPixels[index] = _pixels3[i];
            }
            else if (index < _pixels1.Length+_pixels2.Length+_pixels3.Length+_pixels4.Length)
            {
                var i = index - _pixels1.Length - _pixels2.Length - _pixels3.Length;
                _finalPixels[index] = _pixels4[i];
            }
        }
    }*/
}