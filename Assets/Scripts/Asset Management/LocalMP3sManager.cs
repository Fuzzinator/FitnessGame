using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Pool;
using Cysharp;
using System.Threading;
using TagLib;

public class LocalMP3sManager : MonoBehaviour
{
    public static LocalMP3sManager Instance { get; private set; }
    public static List<string> AvailableMP3Paths { get; private set; }
    public static readonly string[] Mp3Extension = { ".mp3" };
    private static CancellationToken _canecllationToken;
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
        _canecllationToken = this.GetCancellationTokenOnDestroy();
        TryAutoConvert().Forget();
    }

    private static async UniTaskVoid TryAutoConvert()
    {
        if (AvailableMP3Paths == null)
        {
            AvailableMP3Paths = CollectionPool<List<string>, string>.Get();
        }
        else
        {
            AvailableMP3Paths.Clear();
        }
        AssetManager.GetAssetPathsFromAutoConvert(Mp3Extension, AvailableMP3Paths);
        await UniTask.DelayFrame(1, cancellationToken: _canecllationToken);
        for (int i = 0; i < AvailableMP3Paths.Count; i++)
        {
            var songName = TryConvertSong(i, out var download);
            if (download != null)
            {
                download.ProgressUpdated.AddListener((val) => DeleteWhenSongCompletes(AvailableMP3Paths[i], val));
            }
            if (i % 5 == 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(10f), cancellationToken: _canecllationToken);
            }
        }
    }

    private static void DeleteWhenSongCompletes(string toDelete, double val)
    {
        if (val == 1)
        {
            System.IO.File.Delete(toDelete);
            AvailableMP3Paths.Remove(toDelete);
        }
    }


    public static void UpdateAvailableMp3s()
    {
        if (AvailableMP3Paths == null)
        {
            AvailableMP3Paths = CollectionPool<List<string>, string>.Get();
        }
        else
        {
            AvailableMP3Paths.Clear();
        }
        AssetManager.GetAssetPathsFromDownloads(LocalMP3sManager.Mp3Extension, AvailableMP3Paths);
    }

    public static string GetMp3Name(int index)
    {
        var filePath = AvailableMP3Paths[index];
        var path = Path.GetFileName(filePath);
        return path;
    }

    public static bool TryGetMP3Info(int index, out TagLib.File file)
    {
        file = null;
        try
        {
            var filePath = AvailableMP3Paths[index];
            file = TagLib.File.Create(filePath);
            return true;
        }
        catch (Exception ex)
        {
            if(ex is not CorruptFileException)
            {
                Debug.LogError($"{ex.Message}\n{ex.StackTrace}");
            }
            return false;
        }
    }


    public static string TryConvertSong(int index, out BeatSageDownloadManager.Download download)
    {
        if (GameManager.Instance.DemoMode && BeatSageDownloadManager.Downloads.Count + SongInfoFilesReader.Instance.CustomSongsCount >= GameManager.DemoModeMaxCustomSongs)
        {
            var visuals = new Notification.NotificationVisuals(
                        $"Cannot convert song. The maximum number of custom songs in this demo is {GameManager.DemoModeMaxCustomSongs}. To have more custom songs, please consider buying the full game.",
                        "Demo Mode", autoTimeOutTime: 5f, button1Txt: "Okay");
            NotificationManager.RequestNotification(visuals);
            download = null;
            return null;
        }
        var path = AvailableMP3Paths[index];
        var songName = Path.GetFileName(path);
        download = BeatSageDownloadManager.TryAddDownload(songName, path);
        if (download == null)
        {
            return songName;
        }
        return songName;
    }
}
