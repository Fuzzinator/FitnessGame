using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameModeManagement;
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
        if(playlist?.Items == null)
        {
            return false;
        }
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

        if(SongInfoFilesReader.Instance == null || SongInfoFilesReader.Instance.AvailableSongs == null)
        {
            return false;
        }

        foreach (var info in SongInfoFilesReader.Instance.AvailableSongs)
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
            var locationResults = Addressables.LoadResourceLocationsAsync(fileLocation);
            var resourceLocations = await locationResults;
            if (resourceLocations.Count == 0)
            {
                Addressables.Release(locationResults);
                return null;
            }

            var assetResults = Addressables.LoadAssetAsync<TextAsset>(fileLocation);
            var json = await assetResults;
            if (json == null)
            {
                Addressables.Release(locationResults);
                Addressables.Release(assetResults);
                Debug.LogError("Failed to load local resource file");
                return null;
            }

            var songInfo = JsonUtility.FromJson<SongInfo>(json.text);
            Addressables.Release(locationResults);
            Addressables.Release(assetResults);
            return songInfo;
        }

        return null;
    }

    private static async UniTask<bool> AsyncCheckChoreography(PlaylistItem item)
    {
        var targetDifficulty = item.DifficultyEnum == DifficultyInfo.DifficultyEnum.Unset
            ? DifficultyInfo.DifficultyEnum.Normal
            : item.DifficultyEnum;
        var targetGameMode = item.TargetGameMode == GameMode.Unset ? GameMode.Normal : item.TargetGameMode;
        
        var difficultyInfo = item.SongInfo.TryGetActiveDifficultyInfo(targetDifficulty, targetGameMode);
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
            return await AsyncFileCheck(path);
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
            return await AsyncFileCheck(path);
        }
    }

    private static async UniTask<bool> AsyncFileCheck(string path)
    {
        var results = Addressables.LoadResourceLocationsAsync(path);
        var resourceLocations = await results;
        var isValid = resourceLocations.Count > 0;
        Addressables.Release(results);
        return isValid;
    }
}