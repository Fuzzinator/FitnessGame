using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

public class CustomEnvironmentsController : MonoBehaviour
{
    [SerializeField]
    private static List<string> _availableCustomSkyboxes = null;
    [SerializeField]
    private static List<string> _checkedFiles = null;
    [SerializeField]
    private static List<string> _filesToCheck = null;
    [SerializeField]
    private static List<string> _selectedSkyboxFiles = null;
    [SerializeField]
    private static List<string> _customEnvironments = null;


    private const string Png = ".png";
    private const string Jpg = ".jpg";
    private const string Jpeg = ".jpeg";
    //private const string Exr = ".exr"; //For now I wont support Exr but in the future if people request it, I will consider it
    private const string Hdr = ".hdr";

    private const string EnvironmentExtension = ".Env";
    private const string CheckFilesSetting = "CheckedFilesSetting";

    private const int ThumbnailSize = 256;

    public static void ResetCheckedFiles()
    {
        if (_checkedFiles != null)
        {
            CollectionPool<List<string>, string>.Release(_checkedFiles);
        }
        SettingsManager.SetSetting(CheckFilesSetting, _checkedFiles);
    }

    public static List<string> RefreshAvailableCustomEnvironments()
    {
        if (_customEnvironments != null)
        {
            CollectionPool<List<string>, string>.Release(_customEnvironments);
        }
        return GetAvailableCustomEnvironments();
    }

    public static List<string> GetAvailableCustomEnvironments()
    {
        if (_customEnvironments == null)
        {
            _customEnvironments = CollectionPool<List<string>, string>.Get();

            var path = AssetManager.EnvironmentsPath;
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var info = new DirectoryInfo(AssetManager.EnvironmentsPath);
            var files = info.GetFiles();
            foreach (var file in files)
            {
                if (file == null)
                {
                    continue;
                }
                if (string.Equals(file.Extension, EnvironmentExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!_customEnvironments.Contains(file.FullName))
                    {
                        _customEnvironments.Add(file.FullName);
                    }
                }
            }
        }
        return _customEnvironments;
    }

    public static List<string> GetAvailableSkyboxes()
    {
        _availableCustomSkyboxes.Clear();
        var info = new DirectoryInfo(AssetManager.SkyboxesPath);
        var files = info.GetFiles();
        foreach (var file in files)
        {
            if (file == null)
            {
                continue;
            }
            if (string.Equals(file.Extension, Png, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(file.Extension, Jpg, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(file.Extension, Hdr, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!_checkedFiles.Contains(file.FullName))
                {
                    _filesToCheck.Add(file.FullName);
                    _checkedFiles.Add(file.FullName);
                }
                else if (_selectedSkyboxFiles.Contains(file.FullName))
                {
                    if (!_availableCustomSkyboxes.Contains(file.FullName))
                    {
                        _availableCustomSkyboxes.Add(file.FullName);
                    }
                }
            }
        }
        return _availableCustomSkyboxes;
    }

    public static List<string> GetImagePathsInDownloads()
    {
        if (_filesToCheck == null)
        {
            _filesToCheck = CollectionPool<List<string>, string>.Get();
        }
        else
        {
            _filesToCheck.Clear();
        }
        _checkedFiles = SettingsManager.GetSetting<List<string>>(CheckFilesSetting, null, false);
        if (_checkedFiles == null)
        {
            _checkedFiles = CollectionPool<List<string>, string>.Get();
        }

        var info = new DirectoryInfo(AssetManager.DownloadsPath());
        var files = info.GetFiles();
        foreach (var file in files)
        {
            if (file == null)
            {
                continue;
            }
            if (string.Equals(file.Extension, Png, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(file.Extension, Jpg, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(file.Extension, Jpeg, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(file.Extension, Hdr, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!_checkedFiles.Contains(file.FullName))
                {
                    _filesToCheck.Add(file.FullName);
                    _checkedFiles.Add(file.FullName);
                }
                else if (_selectedSkyboxFiles.Contains(file.FullName))
                {
                    if (!_availableCustomSkyboxes.Contains(file.FullName))
                    {
                        _availableCustomSkyboxes.Add(file.FullName);
                    }
                }
            }
        }
        return _checkedFiles;
    }

    public async static UniTask<Sprite> GetEnvironmentThumbnailAsync(string path, CancellationToken token)
    {
        var texture = await AssetManager.LoadImageFromPath(path, token);

        if (texture.width > ThumbnailSize || texture.height > ThumbnailSize)
        {
            texture.Reinitialize(ThumbnailSize, ThumbnailSize);
        }

        var spriteRect = new Rect(Vector2.zero, new Vector2(ThumbnailSize, ThumbnailSize));
        return Sprite.Create(texture, spriteRect, Vector2.zero);
    }

    public static bool TrySetImageAsSkybox(string image, string newName, bool overwriteDuplicates)
    {
        string imageName;
        if (string.IsNullOrWhiteSpace(newName))
        {
            imageName = image.Substring(image.LastIndexOf("/") + 1);
        }
        else
        {
            imageName = newName;
        }
        var newFileLocation = $"{AssetManager.SkyboxesPath}{imageName}";
        if (!overwriteDuplicates && File.Exists(imageName))
        {
            return false;
        }
        File.Move(image, newFileLocation);
        _availableCustomSkyboxes.Add(newFileLocation);
        return true;
    }

    public static async UniTask<bool> TryCreateEnvironment(CustomEnvironment customEnvironment, bool overwriteDuplicates)
    {
        var path = $"{AssetManager.EnvironmentsPath}{customEnvironment.EnvironmentName}{EnvironmentExtension}";

        if (!overwriteDuplicates && File.Exists(path))
        {
            return false;
        }

        using var streamWriter = File.CreateText(path);
        var json = JsonUtility.ToJson(customEnvironment);
        var writingTask = streamWriter.WriteAsync(json);

        await writingTask;

        streamWriter.Close();
        return true;
    }
}
