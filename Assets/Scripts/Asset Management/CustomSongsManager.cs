using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class CustomSongsManager
{
    private const string CustomSongRecord = "CustomSongsRecord.dat";
    private const string DownloadedSongs = "DownloadedSongs";

    private static ES3Settings _songsRecordSettings;
    private static List<string> _songsRecord = null;

    public static ES3Settings SongsRecordSetting => _songsRecordSettings ?? new ES3Settings(CustomSongRecord);

    public static async UniTask<float> TryGetSongLength(SongInfo info,
        CancellationToken token, bool customSong = true)
    {
        float clipLength;
        if (customSong)
        {
            var clipRequest = AssetManager.LoadCustomSong(info.fileLocation, info, token, true);
            var audioClip = await clipRequest;
            if (audioClip == null)
            {
                if (!token.IsCancellationRequested)
                {
                    ErrorReporter.SetSuppressed(true, true);
                    Debug.LogError($"Failed to load {info.SongName}. SongID is: {info.SongID}. File location is {info.fileLocation}");
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
            if(audioClip == null)
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

    public static List<string> GetRecord()
    {
        _songsRecord = ES3.Load(DownloadedSongs, _songsRecord, SongsRecordSetting);
        return _songsRecord;
    }

    public static void AddToRecord(string songID)
    {
        if(_songsRecord == null)
        {
            GetRecord();
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
        _songsRecord.Remove(songID);
        ES3.Save(DownloadedSongs, _songsRecord, SongsRecordSetting);
    }
}