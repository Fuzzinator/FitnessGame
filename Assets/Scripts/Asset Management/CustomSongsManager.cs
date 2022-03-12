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

    private const string SONGINFONAME = "Info.txt";
    private const string ALTSONGINFONAME = "Info.dat";

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

    public static async UniTask GetCustomSongs(CancellationTokenSource cancellationSource)
    {
        var path = Path;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var directories = Directory.GetDirectories(path);
        SongLoader songLoader = null;
        if (directories.Length > 0)
        {
            songLoader = new SongLoader();
        }

        foreach (var dir in directories)
        {
            var info = new DirectoryInfo(dir);
            var files = info.GetFiles();
            foreach (var file in files)
            {
                if (file == null)
                {
                    return;
                }

                if (string.Equals(file.Name, SONGINFONAME, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(file.Name, ALTSONGINFONAME, StringComparison.InvariantCultureIgnoreCase))
                {
                    var streamReader = new StreamReader(file.FullName);
                    var result = await streamReader.ReadToEndAsync().AsUniTask()
                        .AttachExternalCancellation(cancellationSource.Token);

                    var item = JsonUtility.FromJson<SongInfo>(result);

                    streamReader.Close();


                    var updatedMaps = false;
                    if (file.Directory != null)
                    {
                        item.fileLocation = file.Directory.Name;
                        updatedMaps = true;
                    }


                    item.isCustomSong = true;
                    if (item.SongLength < 1)
                    {
                        var songLength = await TryGetSongLength(item, songLoader, cancellationSource);
                        item.SongLength = songLength;
                        updatedMaps = true;
                    }

                    updatedMaps = await item.UpdateDifficultySets(cancellationSource.Token);

                    if (updatedMaps)
                    {
                        await UniTask.DelayFrame(2, cancellationToken: cancellationSource.Token);
                        using (var streamWriter = new StreamWriter(file.FullName))
                        {
                            await streamWriter.WriteAsync(JsonUtility.ToJson(item));
                        }
                    }

                    SongInfoFilesReader.Instance.availableSongs.Add(item);
                }
            }
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


    public static async UniTask<float> TryGetSongLength(SongInfo info, SongLoader songLoader,
        CancellationTokenSource cancellationSource, bool customSong = true)
    {
        if (songLoader != null)
        {
            UniTask<AudioClip> clipRequest;
            if (customSong)
            {
                clipRequest = songLoader.LoadCustomSong(info.fileLocation, info, cancellationSource.Token);
            }
            else
            {
                clipRequest = songLoader.LoadBuiltInSong(info, cancellationSource.Token);
            }

            var audioClip = await clipRequest;
            if (audioClip == null)
            {
                if (!cancellationSource.IsCancellationRequested)
                {
                    Debug.LogError($"Failed to load {info.SongName}");
                }

                return 0;
            }

            return audioClip.length;
        }

        return 0;
    }
}