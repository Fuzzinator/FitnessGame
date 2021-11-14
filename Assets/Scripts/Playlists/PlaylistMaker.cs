using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using UnityEngine.Events;

public class PlaylistMaker : MonoBehaviour, IProgress<float>
{
    public static PlaylistMaker Instance { get; private set; }

    private List<PlaylistItem> _playlistItems = new List<PlaylistItem>();
    private SongInfo _activeItem;

    [SerializeField]
    private UnityEvent _playlistItemsUpdated = new UnityEvent();

    [SerializeField]
    private UnityEvent _startWritingPlaylist = new UnityEvent();

    [SerializeField]
    private UnityEvent<Playlist> _newPlaylistCreated = new UnityEvent<Playlist>();

    public List<PlaylistItem> PlaylistItems => _playlistItems;

    private bool _editMode = false;

    public string PlaylistName => _playlistName;
    private string _playlistName;
    private string _originalName;

    #region Const Strings

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string ANDROIDPATHSTART = "file://";
    private const string PLAYLISTSFOLDER = "/Resources/Playlists/";
#elif UNITY_EDITOR
    private const string UNITYEDITORLOCATION = "/LocalCustomSongs/Playlists/";
#endif

    private const string NEWPLAYLISTNAME = "New Playlist";
    private const string PLAYLISTEXTENSION = ".txt";

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

    public void SetActiveItem(SongInfo info)
    {
        _activeItem = info;
    }

    public static PlaylistItem GetPlaylistItem(SongInfo songInfo, string difficulty)
    {
        return new PlaylistItem(songInfo.SongName, songInfo.fileLocation, difficulty, songInfo.isCustomSong, songInfo);
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

    public void SetPlaylistName(string newName)
    {
        _playlistName = newName;
    }

    public async void CreatePlaylist()
    {
        if (_playlistItems == null || _playlistItems.Count == 0)
        {
            Debug.LogError("Cannot create empty playlist");
            return;
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        var path = $"{Application.persistentDataPath}{PLAYLISTSFOLDER}";
        /*if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }*/
#elif UNITY_EDITOR
        var dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
        var path = $"{dataPath}/{UNITYEDITORLOCATION}";
#endif
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var newPlaylist = new Playlist(_playlistItems, _playlistName);
        if (string.IsNullOrWhiteSpace(_playlistName))
        {
            _playlistName = newPlaylist.PlaylistName;
        }

        var filePath = $"{path}{_playlistName}.txt";
        if (_editMode)
        {
            if (_originalName != _playlistName && File.Exists($"{path}{_originalName}.txt"))
            {
                CustomPlaylistsManager.Instance.DeletePlaylist(_originalName);
            }
        }
        
        if(!_editMode || (_editMode && _originalName != _playlistName))
        {
            var index = 0;
            while (File.Exists(filePath))
            {
                index++;
                filePath = $"{path}{_playlistName}_{index:00}.txt";
                await UniTask.DelayFrame(1);
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

        _newPlaylistCreated?.Invoke(newPlaylist);
        _playlistItems.Clear();
        SetPlaylistName(NEWPLAYLISTNAME);
    }

    public float GetLength()
    {
        var length = 0f;
        foreach (var item in _playlistItems)
        {
            length += item.SongInfo.LengthInMinutes;
        }

        return length;
    }

    public void SetEditMode(bool editMode)
    {
        var playlist = PlaylistManager.Instance.CurrentPlaylist;

        _editMode = editMode;
        if (_editMode)
        {
            _originalName = playlist.PlaylistName;
            _playlistName = playlist.PlaylistName;
            _playlistItems.Clear();
            
            foreach (var item in playlist.Items)
            {
                _playlistItems.Add(item);
            }
            _playlistItemsUpdated?.Invoke();
        }
        else
        {
            _originalName = null;
            _playlistName = string.Empty;
            _playlistItems.Clear();
            _playlistItemsUpdated?.Invoke();
        }
    }

    public void Report(float value)
    {
        Debug.Log(value);
    }
}