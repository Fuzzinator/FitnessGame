using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CustomPlaylistsManager : MonoBehaviour
{
    public static CustomPlaylistsManager Instance { get; private set; }
    
    #region Const Strings

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

    public void TryDeleteActivePlaylist()
    {
        if(PlaylistManager.Instance.CurrentPlaylist == null)
        {
            return;
        }

        var playlist = PlaylistManager.Instance.CurrentPlaylist.PlaylistName;
        var deleteVisuals = new Notification.NotificationVisuals(
            $"Are you sure you would like to permanently delete {playlist}?",
            "Delete Song?", "Confirm", "Cancel");

        NotificationManager.RequestNotification(deleteVisuals, () => DeleteActivePlaylist());
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
        AssetManager.DeletePlaylist(playlist);
        
        PlaylistManager.Instance.CurrentPlaylist = null;
    }
}
