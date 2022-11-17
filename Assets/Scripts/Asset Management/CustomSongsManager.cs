using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class CustomSongsManager
{
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