using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CustomPlaylistsManager : MonoBehaviour
{
    public static CustomPlaylistsManager Instance { get; private set; }
    
    #region Const Strings

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string ANDROIDPATHSTART = "file://";
    private const string PLAYLISTSFOLDER = "/Resources/Playlists/";
#elif UNITY_EDITOR
    private const string UNITYEDITORLOCATION = "/LocalCustomSongs/Playlists/";
#endif

    private const string PLAYLISTEXTENSION = ".txt";
    private const string JPGEXTENSION = ".jpg";

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

    public void DeleteActivePlaylist()
    {
        var playlist = PlaylistManager.Instance.CurrentPlaylist?.PlaylistName;
        if (string.IsNullOrWhiteSpace(playlist))
        {
            Debug.LogWarning("Cannot delete null playlist.");
            return;
        }
        
        PlaylistFilesReader.Instance.RemovePlaylist(PlaylistManager.Instance.CurrentPlaylist);
        DeletePlaylist(playlist);
        
        PlaylistManager.Instance.CurrentPlaylist = null;
    }

    public void DeletePlaylist(string playlistName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var path = $"{Application.persistentDataPath}{PLAYLISTSFOLDER}";
#elif UNITY_EDITOR
        var dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
        var path = $"{dataPath}/{UNITYEDITORLOCATION}";
#endif
        var filePath = $"{path}{playlistName}{PLAYLISTEXTENSION}";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        
        var imagePath = $"{path}{playlistName}{JPGEXTENSION}";
        if (File.Exists(imagePath))
        {
            File.Delete(imagePath);
        }
    }
}
