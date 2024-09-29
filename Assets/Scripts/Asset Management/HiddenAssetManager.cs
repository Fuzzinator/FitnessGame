using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HiddenAssetManager
{
    private static List<string> _hiddenSongs;
    private static List<string> _hiddenPlaylists;
    private const string HiddenSongs = "HiddenSongs";
    private const string HiddenPlaylists = "HiddenPlaylists";
    private static bool _songsInitialized = false;
    private static bool _playlistsInitialized = false;

    private static void InitializeSongs()
    {
        if (!_songsInitialized)
        {
            _hiddenSongs = SettingsManager.GetSetting(HiddenSongs, new List<string>());
            _songsInitialized = true;
        }
    }

    private static void InitializePLaylists()
    {
        if (!_playlistsInitialized)
        {
            _hiddenPlaylists = SettingsManager.GetSetting(HiddenPlaylists, new List<string>());
            _playlistsInitialized = true;
        }
    }

    public static bool IsHiddenSong(string guid)
    {
        InitializeSongs();
        return _hiddenSongs.Contains(guid);
    }

    public static bool IsHiddenPlaylist(string guid)
    {
        InitializePLaylists();
        return _hiddenPlaylists.Contains(guid);
    }

    public static void HideSong(string guid)
    {
        InitializeSongs();
        _hiddenSongs.Add(guid);
        SettingsManager.SetSetting(HiddenSongs, _hiddenSongs);
    }

    public static void HidePlaylist(string guid)
    {
        InitializePLaylists();
        _hiddenPlaylists.Add(guid);
        SettingsManager.SetSetting(HiddenPlaylists, _hiddenPlaylists);
    }

    public static void ResetHiddenSongs()
    {
        InitializeSongs();
        _hiddenSongs.Clear();
        SettingsManager.SetSetting(HiddenSongs, _hiddenSongs);
        SongInfoFilesReader.Instance.ResetHiddenSongs();
    }

    public static void ResetHiddenPlaylists()
    {
        InitializePLaylists();
        _hiddenPlaylists.Clear();
        SettingsManager.SetSetting(HiddenPlaylists, _hiddenPlaylists);
        PlaylistFilesReader.Instance.ResetHiddenPlaylists();
    }
}
