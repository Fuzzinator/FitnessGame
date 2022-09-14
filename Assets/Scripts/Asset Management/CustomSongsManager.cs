using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class CustomSongsManager
{
    #region Const Strings

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string ANDROIDPATHSTART = "file://";
    private const string SONGSFOLDER = "/Resources/Songs/";
#elif UNITY_EDITOR
    private const string UNITYEDITORLOCATION = "/LocalCustomSongs/Songs/";
#endif


    #endregion

    public static string Path
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        var path = $"{Application.persistentDataPath}{SONGSFOLDER}";
#elif UNITY_EDITOR
            var dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
            var path = $"{dataPath}{UNITYEDITORLOCATION}";
#endif
            return path;
        }
    }

    
    public static async UniTask DeleteCustomSong(SongInfo info)
    {
        
        SongInfoFilesReader.Instance.availableSongs.Remove(info);
        var path = $"{Path}/{info.fileLocation}";
        if (!Directory.Exists(path))
        {
            Debug.LogWarning("Invalid path cannot delete");
        }

        await UniTask.RunOnThreadPool(() => Directory.Delete(path, true));
        await SongInfoFilesReader.Instance.UpdateSongs();
    }


    public static async UniTask<float> TryGetSongLength(SongInfo info,
        CancellationToken token, bool customSong = true)
    {
        UniTask<AudioClip> clipRequest;
        if (customSong)
        {
            clipRequest = AssetManager.LoadCustomSong(info.fileLocation, info, token);
        }
        else
        {
            clipRequest = AssetManager.LoadBuiltInSong(info, token);
        }

        var audioClip = await clipRequest;
        if (audioClip == null)
        {
            if (!token.IsCancellationRequested)
            {
                Debug.LogError($"Failed to load {info.SongName}");
            }

            return 0;
        }

        return audioClip.length;
    }
}