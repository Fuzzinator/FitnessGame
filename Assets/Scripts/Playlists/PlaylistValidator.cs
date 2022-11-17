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
        SongInfo songInfo = null; //await AsyncLoadSongInfo(item);

        foreach (var info in SongInfoFilesReader.Instance.availableSongs)
        {
            if (info == item)
            {
                songInfo = info;
                break;
            }
        }
        
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
            var path = $"{AssetManager.SongsPath}{item.FileLocation}{INFO}{DAT}";
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
            var fileLocation = $"{AssetManager.LOCALSONGSFOLDER}{item.FileLocation}{INFO}{TXT}";
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
        var difficultyInfo = item.SongInfo.TryGetActiveDifficultyInfo(item.DifficultyEnum, item.TargetGameMode);
        if (item.IsCustomSong)
        {
            var path = $"{AssetManager.SongsPath}{item.FileLocation}/{difficultyInfo.FileName}";
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

            var path = $"{AssetManager.LOCALSONGSFOLDER}{item.FileLocation}/{txtVersion}";
            var resourceLocations = await Addressables.LoadResourceLocationsAsync(path);
            return resourceLocations.Count > 0;
        }
    }

    private static async UniTask<bool> AsyncCheckSongFile(PlaylistItem item)
    {
        if (item.IsCustomSong)
        {
            var path = $"{AssetManager.SongsPath}{item.FileLocation}/{item.SongInfo.SongFilename}";
            return File.Exists(path);
        }
        else
        {
            var path = $"{AssetManager.LOCALSONGSFOLDER}{item.SongInfo.fileLocation}/{item.SongInfo.SongFilename}";
            var resourceLocations = await Addressables.LoadResourceLocationsAsync(path);
            return resourceLocations.Count > 0;
        }
    }
}