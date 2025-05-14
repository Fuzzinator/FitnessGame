using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BeatSaverSharp;
using BeatSaverSharp.Models;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class CustomSongsManager
{
    private const string CustomSongRecord = "CustomSongsRecord.dat";
    private const string DownloadedSongs = "DownloadedSongs";
    private const string AutoGetSongs = "AutoDownloadSongs";

    private static ES3Settings _songsRecordSettings;
    private static List<string> _songsRecord = null;

    public static ES3Settings SongsRecordSetting => _songsRecordSettings ?? new ES3Settings(CustomSongRecord);

    public static async UniTask<float> TryGetSongLength(SongInfo info,
        CancellationToken token, bool customSong = true)
    {
        float clipLength;
        if (customSong)
        {
            var audioClip = await AssetManager.LoadCustomSong(info.fileLocation, info, token, true);
            if (audioClip == null)
            {
                if (!token.IsCancellationRequested)
                {
                    ErrorReporter.SetSuppressed(true, true);
                    Debug.LogError($"Failed to load {info.SongName}. SongID is: {info.SongID}.The song was{(info.AutoConverted ? string.Empty : "not")}. File location is {info.fileLocation}. Song file is {info.SongFilename}.");
                    ErrorReporter.SetSuppressed(false);
                }

                return 0;
            }
            clipLength = audioClip.length;
        }
        else
        {
            var clipRequest = await AssetManager.LoadBuiltInSong(info, token);
            var audioClip = clipRequest.AudioClip;
            if (audioClip == null)
            {
                Addressables.Release(clipRequest.OperationHandle);
                if (!token.IsCancellationRequested)
                {
                    Debug.LogError($"Failed to load {info.SongName}");
                }

                return 0;
            }
            clipLength = audioClip.length;
            Addressables.Release(clipRequest.OperationHandle);
        }



        return clipLength;
    }

    public static IReadOnlyCollection<string> GetRecord()
    {
        _songsRecord = ES3.Load(DownloadedSongs, new List<string>(), SongsRecordSetting);
        return _songsRecord;
    }

    public static void AddToRecord(string songID)
    {
        if (_songsRecord == null)
        {
            GetRecord();
        }
        if (_songsRecord.Contains(songID))
        {
            return;
        }
        _songsRecord.Add(songID);
        ES3.Save(DownloadedSongs, _songsRecord, SongsRecordSetting);
    }

    public static void RemoveFromRecord(string songID)
    {
        if (_songsRecord == null)
        {
            GetRecord();
        }
        if (!_songsRecord.Contains(songID))
        {
            return;
        }
        _songsRecord.Remove(songID);
        ES3.Save(DownloadedSongs, _songsRecord, SongsRecordSetting);
    }

    private static void RemoveFromRecord(IReadOnlyCollection<string> songIDs)
    {
        if (_songsRecord == null)
        {
            GetRecord();
        }

        foreach (var songID in songIDs)
        {
            if (!_songsRecord.Contains(songID))
            {
                continue;
            }
            _songsRecord.Remove(songID);
        }
        ES3.Save(DownloadedSongs, _songsRecord, SongsRecordSetting);
    }

    public static void ConfirmHasAllSongInRecord()
    {
        var songRecords = GetRecord();
        List<string> songIDsToDownload = null;
        foreach (var songID in songRecords)
        {
            var hasSong = false;
            foreach (var song in SongInfoFilesReader.Instance.AvailableSongs)
            {
                if (string.Equals(song.SongID, songID))
                {
                    hasSong = true;
                    break;
                }
            }
            if (hasSong)
            {
                continue;
            }

            if (songIDsToDownload == null)
            {
                songIDsToDownload = new List<string>();
            }

            songIDsToDownload.Add(songID);
        }

        if (songIDsToDownload == null)
        {
            return;
        }

        ValidateIDs(songIDsToDownload).Forget();
    }

    private static async UniTask<bool> ValidateIDs(List<string> ids)
    {
        List<string> invalidIDs = null;
        var cancellationTokenSource = new CancellationTokenSource();

        await UniTask.SwitchToMainThread(cancellationTokenSource.Token);
        var beatSaver = new BeatSaver(Application.productName, Version.Parse(Application.version));

        for (var i = 0; i < ids.Count; i++)
        {
            var beatmap = await beatSaver.Beatmap(ids[i], cancellationTokenSource.Token);

            if (beatmap == null)
            {
                if (invalidIDs == null)
                {
                    invalidIDs = new List<string>();
                }
                invalidIDs.Add(ids[i]);

                ids.RemoveAt(i);
                i--;
            }
        }

        if (invalidIDs != null)
        {
            HandleRejectSongRecovery(invalidIDs);
        }

        await UniTask.DelayFrame(1);


        var visuals = new Notification.NotificationVisuals(
            "Some of your community content songs appear to be missing, would you like to re-download them?",
            "Missing Songs",
            "Yes",
            "No",
            "Don't Ask Again");
        NotificationManager.RequestNotification(visuals,
            () => HandleSongRecovery(ids.AsReadOnly()),
            () => HandleRejectSongRecovery(ids),
            () => SettingsManager.SetSetting(AutoGetSongs, false));

        return true;
    }

    private static void HandleSongRecovery(IReadOnlyCollection<string> songIDs)
    {
        var beatSaver = new BeatSaver(Application.productName, Version.Parse(Application.version));
        var cancellationTokenSource = new CancellationTokenSource();

        foreach (var songID in songIDs)
        {
            DownloadSong(songID, beatSaver, cancellationTokenSource).Forget();
        }
    }

    private static void HandleRejectSongRecovery(IReadOnlyCollection<string> songIDs)
    {
        RemoveFromRecord(songIDs);
    }

    private static async UniTaskVoid DownloadSong(string songID, BeatSaver beatSaver, CancellationTokenSource cancellationTokenSource)
    {
        var progress = new Progress<double>();
        var beatmap = await beatSaver.Beatmap(songID, cancellationTokenSource.Token);
        if (beatmap == null)
        {
            return;
        }

        byte[] songBytes = await DownloadZip(beatmap, progress, cancellationTokenSource.Token);

        if (songBytes == null || cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        await UniTask.DelayFrame(1);

        if (cancellationTokenSource == null || cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        var folderName = GetFolderName(beatmap);

        await ExtractZip(folderName, songBytes);

        await UniTask.DelayFrame(1);
        await UniTask.SwitchToMainThread(cancellationTokenSource.Token);

        await LoadSong(beatmap, folderName);
    }

    private static async UniTask<byte[]> DownloadZip(Beatmap targetBeatmap, Progress<double> progress, CancellationToken cancellationToken)
    {
        try
        {
            var songBytes = await targetBeatmap.LatestVersion.DownloadZIP(cancellationToken, progress);
            return songBytes;
        }
        catch (Exception ex)
        {
            if (ex is TimeoutException)
            {
                if (targetBeatmap == null)
                {
                    return null;
                }
            }
            else
            {
                Debug.LogError(ex);
            }
        }
        return null;
    }

    private static async UniTask ExtractZip(string folderName, byte[] songBytes)
    {
        try
        {
            if (Directory.Exists(folderName))
            {
                Directory.Delete(folderName, true);
            }
            ZipFileManagement.ExtractAndSaveZippedSong(folderName, songBytes);
        }
        catch (Exception ex)
        {
            Debug.LogError($"{folderName} cant be saved might have illegal characters {ex.Message} -- {ex.StackTrace}");
        }
    }

    private static string GetFolderName(Beatmap beatmap)
    {
        return $"{beatmap.ID} ({beatmap.Metadata.SongName} - {beatmap.Metadata.LevelAuthorName})".RemoveIllegalIOCharacters();
    }

    private static async UniTask LoadSong(Beatmap beatmap, string folderName)
    {
        var score = 0f;
        if (beatmap.Stats != null)
        {
            score = beatmap.Stats.Score;
        }
        await SongInfoFilesReader.Instance.LoadNewSong(folderName, beatmap.ID, score, false);

        PlaylistFilesReader.Instance.RefreshPlaylistsValidStates().Forget();
    }
}