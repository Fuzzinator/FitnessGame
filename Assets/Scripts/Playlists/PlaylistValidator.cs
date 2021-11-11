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
    private const string DAT = ".dat";
#endif

    private const string SONGSFOLDER = "/Resources/Songs/";
    private const string LOCALSONGSFOLDER = "Assets/Music/Songs/";
    private const string INFO = "/Info";
    private const string TXT = ".txt";

    #endregion

    public static async Task<bool> IsValid(Playlist playlist)
    {
        for (var i = 0; i < playlist.Items.Length; i++)
        {
            var item = playlist.Items[i];
            var isValid = IsValid(item);
            await isValid;
            if (!isValid.Result)
            {
                return isValid.Result;
            }
        }

        return true;
    }
    
    public static async Task<bool> IsValid(PlaylistItem item)
    {
        var songInfo = AsyncLoadSongInfo(item);
        await songInfo;
        if (songInfo.Result == null)
        {
            return false;
        }

        item.SongInfo = songInfo.Result;

        var choreographyExists = AsyncCheckChoreography(item);
        await choreographyExists;
        if (!choreographyExists.Result)
        {
            return false;
        }

        var songFileExists = AsyncCheckSongFile(item);
        await songFileExists;
        if (!songFileExists.Result)
        {
            return false;
        }

        return true;
    }

    private static async Task<SongInfo> AsyncLoadSongInfo(PlaylistItem item)
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
            var reading = streamReader.ReadToEndAsync();
            await reading;
            streamReader.Close();
            if (reading.IsCompleted)
            {
                return JsonUtility.FromJson<SongInfo>(reading.Result);
            }
            else
            {
                Debug.LogError("Failed to read song info");
            }
        }
        else
        {
            var location = Addressables.LoadResourceLocationsAsync($"{LOCALSONGSFOLDER}{item.FileLocation}{INFO}{TXT}");
            await location;
            if (location.Result.Count == 0)
            {
                return null;
            }

            var request = Addressables.LoadAssetAsync<TextAsset>($"{LOCALSONGSFOLDER}{item.FileLocation}{INFO}{TXT}");
            await request;
            var json = request.Result;
            if (json == null)
            {
                Debug.LogError("Failed to load local resource file");
                return null;
            }

            return JsonUtility.FromJson<SongInfo>(json.text);
        }

        return null;
    }

    private static async Task<bool> AsyncCheckChoreography(PlaylistItem item)
    {
        var difficultyInfo = item.SongInfo.TryGetActiveDifficultySet(item.Difficulty);
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
            if (txtVersion.EndsWith(DAT))
            {
                txtVersion = txtVersion.Replace(DAT, TXT);
            }

            var path = $"{LOCALSONGSFOLDER}{item.FileLocation}/{txtVersion}";
            var locations = Addressables.LoadResourceLocationsAsync(path);
            await locations;
            return locations.Result.Count > 0;
        }
    }

    private static async Task<bool> AsyncCheckSongFile(PlaylistItem item)
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
            var locations = Addressables.LoadResourceLocationsAsync(path);
            await locations;
            return locations.Result.Count > 0;
        }
    }
}