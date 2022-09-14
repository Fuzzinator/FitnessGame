using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class AssetManager : MonoBehaviour
{
    #region Const Strings

#if UNITY_EDITOR
    private const string PAUSEINEDITOR = "Pause In Editor";
    private const string EDITORCUSTOMSONGFOLDER = "/LocalCustomSongs/Songs/";
    private const string EDITORPLAYLISTLOCATION = "/LocalCustomSongs/Playlists/";
    private static readonly string DataPath = Application.dataPath;
#elif UNITY_ANDROID && !UNITY_EDITOR
    private const string ANDROIDPATHSTART = "file://";
    private static readonly string DataPath = Application.persistentDataPath;
#endif

    private const string PLAYLISTEXTENSION = ".txt";
    private const string SONGSFOLDER = "/Resources/Songs/";
    private const string PLAYLISTSFOLDER = "/Resources/Playlists/";
    private const string LOCALSONGSFOLDER = "Assets/Music/Songs/";

    private const string SONGINFONAME = "Info.txt";
    private const string ALTSONGINFONAME = "Info.dat";

    #endregion

    private static bool CheckPermissions()
    {
#if UNITY_ANDROID
        var readPermission = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead);
        if (!readPermission)
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
            readPermission = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead);
        }

        var writePermission = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite);
        if (!writePermission)
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            writePermission = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead);
        }

        return readPermission && writePermission;
#endif

        return true;
    }

    public static async UniTask<AudioClip> LoadCustomSong(string parentDirectory, SongInfo info,
        CancellationToken cancellationToken)
    {
        if (!CheckPermissions())
        {
            Debug.LogWarning("User did not give permissions cannot access custom files");
            return null;
        }

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
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Debug.LogError($"failed to get audio clip\n {e.Message}");
            return null;
        }
    }

    public static async UniTask<AudioClip> LoadBuiltInSong(SongInfo item, CancellationToken cancellationToken)
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

    public static async UniTask<Texture2D> LoadCustomSongImage(string parentDirectory, SongInfo info,
        CancellationToken cancellationToken)
    {
        if (!CheckPermissions())
        {
            Debug.LogWarning("User did not give permissions cannot access custom files");
            return null;
        }
        /*try
        {*/
#if UNITY_ANDROID && !UNITY_EDITOR
            var path = $"{DataPath}{SONGSFOLDER}{parentDirectory}/{info.ImageFilename}";
#elif UNITY_EDITOR
            var path = DataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            path = $"{path}{EDITORCUSTOMSONGFOLDER}{parentDirectory}/{info.ImageFilename}";
#endif
            if (!File.Exists(path))
            {
                Debug.LogError($"No image found at\"{path}\"");
                return null;
            }

            var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
            if (bytes == null || bytes.Length == 0)
            {
                Debug.LogError($"Failed to load image at \"{path}\"");
                return null;
            }

            await UniTask.SwitchToMainThread(cancellationToken);
            var texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            return texture;
        /*}
        catch (Exception e) when (e is OperationCanceledException)
        {
            return null;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Debug.LogError($"Failed to load image\n {e.Message}");
            return null;
        }*/
    }

    public static async UniTask<Texture2D> LoadBuiltInSongImage(SongInfo item, CancellationToken cancellationToken)
    {
        try
        {
            var fileName = item.ImageFilename;
            var request = Addressables.LoadAssetAsync<Texture2D>($"{LOCALSONGSFOLDER}{item.fileLocation}/{fileName}");
            await request.ToUniTask(cancellationToken: cancellationToken);

            var texture = request.Result;
            if (texture == null)
            {
                Debug.LogError("Failed to load local resource file");
                return null;
            }

            texture.name = item.SongName;
            return texture;
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
            return null;
        }
    }

    public static async UniTask GetCustomPlaylists(Action<Playlist> playlistLoaded)
    {
        if (!CheckPermissions())
        {
            Debug.LogWarning("User did not give permissions cannot access custom files");
            return;
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        var path = $"{Application.persistentDataPath}{PLAYLISTSFOLDER}";
        /*if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }*/
#elif UNITY_EDITOR
        var dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
        var path = $"{dataPath}{EDITORPLAYLISTLOCATION}";
#endif
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var info = new DirectoryInfo(path);
        var files = info.GetFiles();

        foreach (var file in files)
        {
            if (file.Extension == PLAYLISTEXTENSION)
            {
                var streamReader = new StreamReader(file.FullName);
                var reading = streamReader.ReadToEndAsync();
                await reading;
                var playlist = JsonUtility.FromJson<Playlist>(reading.Result);

                streamReader.Close();

                playlist.isValid = await PlaylistValidator.IsValid(playlist);
                playlistLoaded?.Invoke(playlist);
                //if (playlist.isValid)
                //{
                //availablePlaylists.Add(playlist);
                //}
            }
        }
    }
    
    public static async UniTask GetBuiltInPlaylists(string label, Action<Playlist> playlistLoaded)
    {
        await Addressables.LoadAssetsAsync<TextAsset>(label, async asset =>
        {
            if (asset == null)
            {
                return;
            }

            var playlist = JsonUtility.FromJson<Playlist>(asset.text);
            playlist.isValid = await PlaylistValidator.IsValid(playlist); //This is a temporary solution.
            playlistLoaded?.Invoke(playlist);
        });
    }
    
    public static async UniTask GetBuiltInSongs(AssetLabelReference label, Action<SongInfo> songLoaded)
    {
        await Addressables.LoadAssetsAsync<TextAsset>(label, asset =>
        {
            if (asset == null)
            {
                return;
            }

            var item = JsonUtility.FromJson<SongInfo>(asset.text);
            item.isCustomSong = false;

            songLoaded?.Invoke(item);
            //availableSongs.Add(item);
        });
    }

    public static async UniTask GetCustomSongs(Action<SongInfo> songLoaded, CancellationTokenSource cancellationSource)
    {
        if (!CheckPermissions())
        {
            Debug.LogWarning("User did not give permissions cannot access custom files");
            return;
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        var path = $"{Application.persistentDataPath}{SONGSFOLDER}";
#elif UNITY_EDITOR
        var dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
        var path = $"{dataPath}{EDITORCUSTOMSONGFOLDER}";
#endif
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var directories = Directory.GetDirectories(path);

        foreach (var dir in directories)
        {
            var item = await GetSingleCustomSong(dir, cancellationSource.Token);
            if (item != null)
            {
                songLoaded?.Invoke(item);
            }
        }
    }

    public static async UniTask<SongInfo> TryGetSingleCustomSong(string fileLocation, CancellationToken token)
    {
        if (!CheckPermissions())
        {
            Debug.LogWarning("User did not give permissions cannot access custom files");
            return null;
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        var path = $"{Application.persistentDataPath}{SONGSFOLDER}/{fileLocation}";
#elif UNITY_EDITOR
        var dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
        var path = $"{dataPath}{EDITORCUSTOMSONGFOLDER}/{fileLocation}";
#endif
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return await GetSingleCustomSong(fileLocation, token);
    }

    public static async UniTask<SongInfo> GetSingleCustomSong(string fileLocation, CancellationToken token)
    {
        var info = new DirectoryInfo(fileLocation);
        var files = info.GetFiles();
        foreach (var file in files)
        {
            if (file == null)
            {
                return null;
            }

            if (string.Equals(file.Name, SONGINFONAME, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(file.Name, ALTSONGINFONAME, StringComparison.InvariantCultureIgnoreCase))
            {
                var streamReader = new StreamReader(file.FullName);
                var result = await streamReader.ReadToEndAsync().AsUniTask()
                    .AttachExternalCancellation(token);

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
                    var songLength = await CustomSongsManager.TryGetSongLength(item, token);
                    item.SongLength = songLength;
                    updatedMaps = true;
                }

                updatedMaps = await item.UpdateDifficultySets(token);

                if (updatedMaps)
                {
                    await UniTask.DelayFrame(2, cancellationToken: token);
                    using (var streamWriter = new StreamWriter(file.FullName))
                    {
                        await streamWriter.WriteAsync(JsonUtility.ToJson(item));
                    }
                }
                return item;
            }
        }

        return null;
    }
}