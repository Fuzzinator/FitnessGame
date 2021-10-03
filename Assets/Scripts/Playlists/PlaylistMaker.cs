using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

public class PlaylistMaker : MonoBehaviour
{
    public static PlaylistMaker Instance { get; private set; }

    private List<PlaylistItem> _playlistItems = new List<PlaylistItem>();
    private SongInfo _activeItem;

    [SerializeField]
    private UnityEvent _playlistItemsUpdated = new UnityEvent();
    
    [SerializeField]
    private UnityEvent<Playlist> _newPlaylistCreated = new UnityEvent<Playlist>();

    public List<PlaylistItem> PlaylistItems => _playlistItems;

    #region Const Strings

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string ANDROIDPATHSTART = "file://";
    private const string PLAYLISTSFOLDER = "/Resources/Playlists/";
#elif UNITY_EDITOR
    private const string UNITYEDITORLOCATION = "E:\\Projects\\FitnessGame\\LocalCustomSongs\\Playlists";
#endif

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

    public PlaylistItem GetPlaylistItem(string difficulty)
    {
        return new PlaylistItem(_activeItem.SongName, _activeItem.fileLocation, difficulty, _activeItem.isCustomSong);
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

    public async void CreatePlaylist()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var path = $"{ANDROIDPATHSTART}{Application.persistentDataPath}{PLAYLISTSFOLDER}";
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
#elif UNITY_EDITOR
        var path = UNITYEDITORLOCATION;
#endif
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        var newPlaylist = new Playlist(_playlistItems);
        var streamWriter = File.CreateText($"{path}{newPlaylist.PlaylistName}.txt");
        await streamWriter.WriteAsync(JsonUtility.ToJson(newPlaylist));
        
        _newPlaylistCreated?.Invoke(newPlaylist);
    }
}