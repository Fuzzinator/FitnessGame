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

    public void DeleteActivePlaylist()
    {
        var playlist = PlaylistManager.Instance.CurrentPlaylist.PlaylistName;
        
        DeletePlaylist(playlist);
        
        PlaylistManager.Instance.CurrentPlaylist = new Playlist();
    }

    private void DeletePlaylist(string playlistName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var path = $"{Application.persistentDataPath}{PLAYLISTSFOLDER}";
#elif UNITY_EDITOR
        var path = UNITYEDITORLOCATION;
#endif
        path = $"{path}{playlistName}{PLAYLISTEXTENSION}";
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        PlaylistFilesReader.Instance.UpdatePlaylists();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
