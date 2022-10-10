using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class PlaylistValidator
{
    #region Const Strings

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string ANDROIDPATHSTART = "file://";
#elif UNITY_EDITOR
    private const string UNITYEDITORLOCATION = "/LocalCustomSongs/Songs/";
#endif

    private const string SONGSFOLDER = "/Resources/Songs/";
    private const string LOCALSONGSFOLDER = "Assets/Music/Songs/";
    private const string INFO = "/Info";
    private const string TXT = ".txt";
    private const string DAT = ".dat";

    #endregion

    public static async UniTask<bool> IsValid(Playlist playlist)
    {
        for (var i = 0; i < playlist.Items.Length; i++)
        {
            var item = playlist.Items[i];
            var isValid = await IsValid(item);
            if (!isValid)
            {
                return isValid;
            }
        }

        return true;
    }
    
    public static async UniTask<bool> IsValid(PlaylistItem item)
    {
        var songInfo = await AsyncLoadSongInfo(item);
        if (songInfo == null)
        {
            return false;
        }

        item.SongInfo = songInfo;

        var choreographyExists = await AsyncCheckChoreography(item);
        if (!choreographyExists)
        {
            return false;
        }

        var songFileExists = await AsyncCheckSongFile(item);
        if (!songFileExists)
        {
            return false;
        }

        return true;
    }

    private static async UniTask<SongInfo> AsyncLoadSongInfo(PlaylistItem item)
    {
        if (item.IsCustomSong)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var path = $"{Application.persistentDataPath}{SONGSFOLDER}{item.FileLocation}/{INFO}.dat";
#elif UNITY_EDITOR

            var dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
            var path = $"{dataPath}{UNITYEDITORLOCATION}{item.FileLocation}{INFO}{DAT}";
#endif
            if (!File.Exists(path))
            {
                Debug.Log(path + " Doesnt Exist.");
                return null;
            }

            var streamReader = new StreamReader(path);
            var json = await streamReader.ReadToEndAsync();
            streamReader.Close();
            if (!string.IsNullOrWhiteSpace(json))
            {
                return JsonUtility.FromJson<SongInfo>(json);
            }
            else
            {
                Debug.LogWarning($"Failed to read song info for {item.SongName}");
            }
        }
        else
        {
            var fileLocation = $"{LOCALSONGSFOLDER}{item.FileLocation}{INFO}{TXT}";
            var resourceLocations = await Addressables.LoadResourceLocationsAsync(fileLocation);
            if (resourceLocations.Count == 0)
            {
                return null;
            }

            var json = await Addressables.LoadAssetAsync<TextAsset>(fileLocation);
            if (json == null)
            {
                Debug.LogError("Failed to load local resource file");
                return null;
            }

            return JsonUtility.FromJson<SongInfo>(json.text);
        }

        return null;
    }

    private static async UniTask<bool> AsyncCheckChoreography(PlaylistItem item)
    {
        var difficultyInfo = item.SongInfo.TryGetActiveDifficultyInfo(item.Difficulty, item.TargetGameMode);
        if (item.IsCustomSong)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var path =
 $"{Application.persistentDataPath}{SONGSFOLDER}{item.FileLocation}/{difficultyInfo.FileName}";
#elif UNITY_EDITOR
            var dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
            var path = $"{dataPath}{UNITYEDITORLOCATION}{item.FileLocation}/{difficultyInfo.FileName}";
#endif
            return File.Exists(path);
        }
        else
        {
            var txtVersion = difficultyInfo.FileName;
            if (string.IsNullOrWhiteSpace(txtVersion))
            {
                return false;
            }
            if (txtVersion.EndsWith(DAT))
            {
                txtVersion = txtVersion.Replace(DAT, TXT);
            }

            var path = $"{LOCALSONGSFOLDER}{item.FileLocation}/{txtVersion}";
            var resourceLocations = await Addressables.LoadResourceLocationsAsync(path);
            return resourceLocations.Count > 0;
        }
    }

    private static async UniTask<bool> AsyncCheckSongFile(PlaylistItem item)
    {
        if (item.IsCustomSong)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var path = $"{Application.persistentDataPath}{SONGSFOLDER}{item.FileLocation}/{item.SongInfo.SongFilename}";
#elif UNITY_EDITOR
            var path = Application.dataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            path = $"{path}{UNITYEDITORLOCATION}{item.FileLocation}/{item.SongInfo.SongFilename}";
#endif
            return File.Exists(path);
        }
        else
        {
            var path = $"{LOCALSONGSFOLDER}{item.SongInfo.fileLocation}/{item.SongInfo.SongFilename}";
            var resourceLocations = await Addressables.LoadResourceLocationsAsync(path);
            return resourceLocations.Count > 0;
        }
    }
}