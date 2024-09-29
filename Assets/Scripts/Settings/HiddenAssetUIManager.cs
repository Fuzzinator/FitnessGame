using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenAssetUIManager : MonoBehaviour, ISaver
{
    private bool _resetSongsRequested = false;
    private bool _resetPlaylistsRequested = false;

    public bool SaveRequested { get; set; }

    public void ResetHiddenSongs()
    {
        _resetSongsRequested = true;
        SaveRequested = true;
        SettingsDisplay.Instance.ChangeWasMade(this);
    }

    public void ResetHiddenPlaylists()
    {
        _resetPlaylistsRequested = true;
        SaveRequested = true;
        SettingsDisplay.Instance.ChangeWasMade(this);
    }

    public void Save(Profile overrideProfile = null)
    {
        if(_resetSongsRequested)
        {
            HiddenAssetManager.ResetHiddenSongs();
        }
        if(_resetPlaylistsRequested)
        {
            HiddenAssetManager.ResetHiddenPlaylists();
        }

        _resetSongsRequested = false;
        _resetPlaylistsRequested = false;
        SaveRequested = false;
    }

    public void Revert()
    {
        _resetSongsRequested = false;
        _resetPlaylistsRequested = false;
        SaveRequested = false;
    }
}
