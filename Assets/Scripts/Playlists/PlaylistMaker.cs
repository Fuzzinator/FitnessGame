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

public class PlaylistMaker : MonoBehaviour, IProgress<float>
{
    public static PlaylistMaker Instance { get; private set; }

    private List<PlaylistItem> _playlistItems = new List<PlaylistItem>();
    private SongInfo _activeItem;
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
    private HitSideType _startingSide = HitSideType.Right;

    [SerializeField]
    private Texture2D _emptyTexture;
    
    public List<PlaylistItem> PlaylistItems => _playlistItems;

    private bool _editMode = false;

    public string PlaylistName => _playlistName;
    public GameMode TargetGameMode => _gameMode;
    public DifficultyInfo.DifficultyEnum Difficulty => _difficulty;

    public HitSideType StartingSide => _startingSide;

    private string _playlistName;
    private string _originalName;
    private string _targetEnv;

    private CancellationToken _cancallationToken;
    
    #region Const Strings

    private const string NEWPLAYLISTNAME = "New Playlist";
    private const string PLAYLISTEXTENSION = ".txt";

    private const int MINUTE = 60;
    private const string DIVIDER = ":";

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

    public static PlaylistItem GetPlaylistItem(SongInfo songInfo, string difficulty,
        DifficultyInfo.DifficultyEnum difficultyEnum, GameMode gameMode)
    {
        return new PlaylistItem(songInfo.SongName, songInfo.fileLocation, difficulty, difficultyEnum,
            songInfo.isCustomSong, gameMode, songInfo);
    }

    public void AddPlaylistItem(PlaylistItem item)
    {
        _playlistItems.Add(item);
        _playlistItemsUpdated?.Invoke();
    }

    public void RemovePlaylistItem(PlaylistItem item)
    {
        if (!_playlistItems.Contains(item))
        {
            Debug.LogWarning("Playlist not contained but trying to remove it. This shouldnt happen.");
            return;
        }

        _playlistItems.Remove(item);
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

        _playlistItemsUpdated?.Invoke();
    }

    public void ShufflePlaylistItems()
    {
        _playlistItems.Shuffle();
    }

    public void SetPlaylistName(string newName)
    {
        _playlistName = newName;
    }

    public void SetTargetEnvironment(string envName)
    {
        _targetEnv = envName;
    }

    public void SetStartingType(HitSideType startingSide)
    {
        _startingSide = startingSide;
    }

    public void CreatePlaylist()
    {
        CreatePlaylistAsync().Forget();
    }

    private async UniTaskVoid CreatePlaylistAsync()
    {
        var sprite = await GetSprite();
        var newPlaylist = new Playlist(_playlistItems, _gameMode, _difficulty, _startingSide, _playlistName, true, _targetEnv, sprite);
        PlaylistManager.Instance.CurrentPlaylist = newPlaylist;
        
        if (_playlistItems == null || _playlistItems.Count == 0)
        {
            //Debug.LogError("Cannot create empty playlist");
            var visuals = new Notification.NotificationVisuals("Cannot create empty playlist", "Failed to Save",
                autoTimeOutTime: 2.5f, popUp: true);
            NotificationManager.RequestNotification(visuals);
            return;
        }
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
            if (_originalName != _playlistName && File.Exists($"{AssetManager.PlaylistsPath}{_originalName}.txt"))
            {
                PlaylistFilesReader.Instance.RemovePlaylistByName(_originalName);
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
        combinedTexture.SetPixels(0,0, 256, 256, texture1.GetPixels(), 0);
        combinedTexture.SetPixels(256,0, 256, 256, texture2.GetPixels(), 0);
        combinedTexture.SetPixels(0,256, 256, 256, texture3.GetPixels(), 0);
        combinedTexture.SetPixels(256,256, 256, 256, texture4.GetPixels(), 0);
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

        var minutes = (int) Mathf.Floor(length / MINUTE);
        var seconds = (int) Mathf.Floor(length % MINUTE);
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

            _playlistItems.Clear();
            _playlistItems.AddRange(playlist.Items);

            _playlistItemsUpdated?.Invoke();
        }
        else
        {
            _originalName = null;
            _playlistName = string.Empty;
            _playlistItems.Clear();
            _playlistItemsUpdated?.Invoke();
            _gameMode = GameMode.Unset;
            _difficulty = DifficultyInfo.DifficultyEnum.Unset;
            _startingSide = HitSideType.Right;
        }
    }

    public void SetDifficulty(DifficultyInfo.DifficultyEnum difficultyEnum)
    {
        _difficulty = difficultyEnum;
    }

    public void SetGameMode(GameMode gameMode)
    {
        _gameMode = gameMode;
    }

    public void Report(float value)
    {
        Debug.Log(value);
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