using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

public class SongLoader
{
    #region Const Strings

    private const string SELECT = "Select";
    private const string MENUBUTTON = "Menu Button";
#if UNITY_EDITOR
    private const string PAUSEINEDITOR = "Pause In Editor";
    private const string EDITORCUSTOMSONGFOLDER = "/LocalCustomSongs/Songs/";
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string ANDROIDPATHSTART = "file://";
#endif

    private const string SONGSFOLDER = "/Resources/Songs/";
    private const string LOCALSONGSFOLDER = "Assets/Music/Songs/";

    #endregion
    
    public async UniTask<AudioClip> LoadCustomSong(string parentDirectory, SongInfo info, CancellationToken cancellationToken)
    {
        try
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var path =
 $"{ANDROIDPATHSTART}{Application.persistentDataPath}{SONGSFOLDER}{parentDirectory}/{info.SongFilename}";
#elif UNITY_EDITOR
            var path = Application.dataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            path = $"{path}{EDITORCUSTOMSONGFOLDER}{parentDirectory}/{info.SongFilename}";
#endif

            var uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.OGGVORBIS);
            ((DownloadHandlerAudioClip) uwr.downloadHandler).streamAudio = true;
            var request = uwr.SendWebRequest();
            await request.ToUniTask(cancellationToken: cancellationToken);
            
            if (uwr.isDone && uwr.result == UnityWebRequest.Result.Success)
            {
                var clip = DownloadHandlerAudioClip.GetContent(uwr);
                clip.name = info.SongName;
                return clip;
            }
            else
            {
                Debug.LogError("failed to get audio clip");
                return null;
            }
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
            return null;
        }
    }

    public async UniTask<AudioClip> LoadBuiltInSong(SongInfo item, CancellationToken cancellationToken)
    {
        try
        {
            var fileName = item.SongFilename;
            var request = Addressables.LoadAssetAsync<AudioClip>($"{LOCALSONGSFOLDER}{item.fileLocation}/{fileName}");
            await request.ToUniTask(cancellationToken: cancellationToken);
            
            var clip = request.Result;
            if (clip == null)
            {
                Debug.LogError("Failed to load local resource file");
                return null;
            }

            clip.name = item.SongName;
            return clip;
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
            return null;
        }
    }
}
