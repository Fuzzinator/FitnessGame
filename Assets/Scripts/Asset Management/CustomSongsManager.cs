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
    public static async UniTask<float> TryGetSongLength(SongInfo info,
        CancellationToken token, bool customSong = true)
    {
        float clipLength;
        if (customSong)
        {
            var clipRequest = AssetManager.LoadCustomSong(info.fileLocation, info, token);
            var audioClip = await clipRequest;
            if (audioClip == null)
            {
                if (!token.IsCancellationRequested)
                {
                    ErrorReporter.SetSuppressed(true, true);
                    Debug.LogError($"Failed to load {info.SongName}. SongID is: {info.SongID}. File location is {info.fileLocation}");
                    ErrorReporter.SetSuppressed(false);
                    var visuals = new Notification.NotificationVisuals($"The song {info.SongName} has failed to load it's information and may be corrupted. Re-downloadingmay resolve this issue.", "Failed Loading Song Info", "Okay");
                    NotificationManager.RequestNotification(visuals);
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
}