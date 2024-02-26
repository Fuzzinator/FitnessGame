using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Cysharp.Threading.Tasks.Linq;
using Superla.RadianceHDR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using UnityEngine.Events;
using UnityEngine.Pool;

public class CustomEnvironmentsController : MonoBehaviour
{
    [SerializeField]
    private static List<string> _availableCustomSkyboxPaths = new List<string>();
    [SerializeField]
    private static List<string> _availableCustomSkyboxNames = new List<string>();
    [SerializeField]
    private static List<string> _availableCustomSkyboxDepthPaths = new List<string>();
    [SerializeField]
    private static List<string> _availableCustomSkyboxDepthNames = new List<string>();
    [SerializeField]
    private static List<string> _imagesInDownloads = null;
    [SerializeField]
    private static List<string> _customEnvironments = null;

    private static List<CustomEnvironment> _availableCustomEnvironments = new List<CustomEnvironment>();
    private static Dictionary<string, Sprite> _skyboxThumbnails = null;
    public static UnityEvent CustomEnvironmentsUpdated { get; private set; } = new UnityEvent();
    public static int CustomSkyboxesCount => _availableCustomSkyboxPaths?.Count ?? 0;
    public static int CustomEnvironmentCount => _availableCustomEnvironments?.Count ?? 0;
    public static int ImagesInDownloadsCount => _imagesInDownloads?.Count ?? 0;

    public static readonly Notification.NotificationVisualInfo ConfirmDeleteSkybox = new Notification.NotificationVisualInfo
    {
        button1Txt = "Delete",
        button2Txt = "Cancel",
        disableUI = true,
        header = "Delete Skybox?",
        message = "Are you sure you would like to delete this skybox? This cannot be undone."
    };

    public static readonly Notification.NotificationVisualInfo ConfirmDeleteEnvironment = new Notification.NotificationVisualInfo
    {
        button1Txt = "Delete",
        button2Txt = "Cancel",
        disableUI = true,
        header = "Delete Environment?",
        message = "Are you sure you would like to delete this environment? This cannot be undone."
    };

    private static readonly string[] _imageExtensions = { ".png", ".bmp", ".jpg", ".jpeg", ".hdr" };
    private const string Png = ".png";
    private const string Bmp = ".bmp";
    private const string Jpg = ".jpg";
    private const string Jpeg = ".jpeg";
    //private const string Exr = ".exr"; //For now I wont support Exr but in the future if people request it, I will consider it
    private const string Hdr = ".hdr";

    private const string EnvironmentExtension = ".Env";
    private const string DepthSkyboxIdentifier = "depth";

    private const int ThumbnailSize = 256;

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
        }
        else
        {
            _customEnvironments.Clear();
        }
        var path = AssetManager.EnvironmentsPath;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            return _customEnvironments;
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
                var name = file.FullName;
                if (!_customEnvironments.Contains(name))
                {
                    _customEnvironments.Add(name);
                }
            }
        }
        return _customEnvironments;
    }

    public static List<string> GetAvailableSkyboxes(bool depth = false)
    {
        if (depth)
        {
            _availableCustomSkyboxDepthPaths.Clear();
            _availableCustomSkyboxDepthNames.Clear();
        }
        else
        {
            _availableCustomSkyboxPaths.Clear();
            _availableCustomSkyboxNames.Clear();
        }

        var path = AssetManager.SkyboxesPath;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            return depth ? _availableCustomSkyboxDepthPaths : _availableCustomSkyboxPaths;
        }

        var info = new DirectoryInfo(path);

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
                string.Equals(file.Extension, Bmp, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(file.Extension, Hdr, StringComparison.InvariantCultureIgnoreCase))
            {
                if ((depth && !file.Name.Contains(DepthSkyboxIdentifier, StringComparison.InvariantCultureIgnoreCase)) ||
                   (!depth && file.Name.Contains(DepthSkyboxIdentifier, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }
                var fullName = file.FullName.Replace('\\', '/');
                if (depth)
                {
                    if (!_availableCustomSkyboxDepthPaths.Contains(fullName))
                    {
                        _availableCustomSkyboxDepthPaths.Add(fullName);
                        _availableCustomSkyboxDepthNames.Add(file.Name);
                    }
                }
                else
                {
                    if (!_availableCustomSkyboxPaths.Contains(fullName))
                    {
                        _availableCustomSkyboxPaths.Add(fullName);
                        _availableCustomSkyboxNames.Add(file.Name);
                    }
                }
            }
        }
        return depth ? _availableCustomSkyboxDepthPaths : _availableCustomSkyboxPaths;
    }
#if UNITY_ANDROID
    private static AndroidJavaObject GetContentResolver()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        return currentActivity.Call<AndroidJavaObject>("getContentResolver");
    }
#endif

    public static List<string> GetImagePathsInDownloads()
    {
        if (_imagesInDownloads == null)
        {
            _imagesInDownloads = CollectionPool<List<string>, string>.Get();
        }
        else
        {
            _imagesInDownloads.Clear();
        }
        AssetManager.GetAssetPathsFromDownloads(_imageExtensions, _imagesInDownloads);
        return _imagesInDownloads;
        //
        // Android 11 is fucking stupid and breaks loading files from the downloads folder unless they are images.
        // This is an attempt at fixing it by using the MediaStore.Downloads functionality but it returns with a cursor with a count of 0
        //
        //
        /*#if UNITY_ANDROID //&& !UNITY_EDITOR
                AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment");
                AndroidJavaObject externalStorageDirectory = environment.CallStatic<AndroidJavaObject>("getExternalStorageDirectory");
                string downloadsFolderPath = externalStorageDirectory.Call<string>("getAbsolutePath") + "/Download";

                AndroidJavaClass downloadsMediaStore = new AndroidJavaClass("android.provider.MediaStore$Downloads");

                AndroidJavaObject contentResolver = GetContentResolver();
                AndroidJavaObject uri = downloadsMediaStore.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI");
                string[] projection = { "_id", "_display_name", "_data", "title" };

                AndroidJavaObject cursor = contentResolver.Call<AndroidJavaObject>("query", uri, projection, null, null, null);

                List<string> fileList = new List<string>();

                if (cursor != null && cursor.Call<bool>("moveToFirst"))
                {

                    while (cursor.Call<bool>("moveToNext"))
                    {
                        int dataIndex = cursor.Call<int>("getColumnIndexOrThrow", "_data");
                        string filePath = cursor.Call<string>("getString", dataIndex);
                        fileList.Add(filePath);
                    }
                }

                cursor.Call("close");
        #else*/
        /*var info = new DirectoryInfo(AssetManager.DownloadsPath());
        var files = info.EnumerateFiles();
        foreach (var file in files)
        {
            if (file == null)
            {
                continue;
            }
            if (string.Equals(file.Extension, Png, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(file.Extension, Jpg, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(file.Extension, Jpeg, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(file.Extension, Bmp, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(file.Extension, Hdr, StringComparison.InvariantCultureIgnoreCase))
            {
                var fullName = file.FullName.Replace('\\', '/');
                if (!_imagesInDownloads.Contains(fullName))
                {
                    _imagesInDownloads.Add(fullName);
                }
            }
        }*/
        //#endif
    }

    public static async UniTask<Texture2D> LoadEnvironmentTexture(string imagePath, CancellationToken token)
    {
        var extension = Path.GetExtension(imagePath);
        Texture2D texture;
        if (string.Equals(extension, Hdr, StringComparison.InvariantCultureIgnoreCase))
        {
            var fileData = await File.ReadAllBytesAsync(imagePath, cancellationToken: token);
            var radianceTexture = new RadianceHDRTexture(fileData);
            texture = radianceTexture.texture;
        }
        else
        {
            texture = await AssetManager.LoadImageFromPath(imagePath, token);
        }
        return texture;
    }

    public static async UniTask SaveSkyboxThumbnail(string imageName, string imageLocation, CancellationToken token)
    {
        var texture = await LoadEnvironmentTexture(imageLocation, token);
        var thumbnailPath = $"{AssetManager.DataPath}{AssetManager.CustomSkyboxThumbnailsFolder}{imageName}.png";

        await SaveSkyboxThumbnail(texture, thumbnailPath, token);
    }

    public static async UniTask SaveSkyboxThumbnail(Texture2D texture, string imagePath, CancellationToken token)
    {
        if (texture == null)
        {
            Debug.LogWarning("Failed to load environment thumbnail");
        }
        if (texture.width > ThumbnailSize || texture.height > ThumbnailSize)
        {
            texture = texture.ScaleTexture(ThumbnailSize, ThumbnailSize, texture.format);
        }

        var bytes = texture.EncodeToPNG();
        var thumbnailPath = AssetManager.SkyboxThumbnailPath;
        if (!Directory.Exists(thumbnailPath))
        {
            Directory.CreateDirectory(thumbnailPath);
        }

        await File.WriteAllBytesAsync(imagePath, bytes);
    }

    public async static UniTask<Sprite> GetEnvironmentThumbnailAsync(string imageName, string imagePath, CancellationToken token, bool cacheSprite = true)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return null;
        }
        var path = imagePath;
        if (cacheSprite && _skyboxThumbnails != null && _skyboxThumbnails.TryGetValue(imagePath, out var skyboxThumbnail))
        {
            var thumbnails = _skyboxThumbnails;
            await UniTask.WaitUntil(() => TryGetThumbnail(imageName, out skyboxThumbnail), cancellationToken: token);
            return skyboxThumbnail;
        }
        else if (cacheSprite)
        {
            if (_skyboxThumbnails == null)
            {
                _skyboxThumbnails = new Dictionary<string, Sprite>();
            }
            if (_skyboxThumbnails.TryGetValue(imagePath, out var thumbnail) && thumbnail != null)
            {
                return thumbnail;
            }
            _skyboxThumbnails[imagePath] = null;
        }

        if (!string.IsNullOrWhiteSpace(imageName))
        {
            var tempPath = $"{AssetManager.SkyboxThumbnailPath}{imageName}";

            if (File.Exists(tempPath))
            {
                path = tempPath;
            }
        }

        var texture = await LoadEnvironmentTexture(path, token);
        if (texture == null)
        {
            return null;
        }
        if (texture.width > ThumbnailSize || texture.height > ThumbnailSize)
        {
            texture = texture.ScaleTexture(ThumbnailSize, ThumbnailSize, texture.format);
        }
        var spriteRect = new Rect(Vector2.zero, new Vector2(texture.width, texture.height));
        var sprite = Sprite.Create(texture, spriteRect, Vector2.zero);
        if (_skyboxThumbnails == null)
        {
            _skyboxThumbnails = new Dictionary<string, Sprite>();
        }
        if (cacheSprite)
        {
            _skyboxThumbnails[imageName] = sprite;
        }
        return sprite;
    }

    public static bool TrySetImageAsSkybox(string image, string newName, bool overwriteDuplicates, CancellationToken token)
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
        if (!Directory.Exists(AssetManager.SkyboxesPath))
        {
            Directory.CreateDirectory(AssetManager.SkyboxesPath);
        }
#if UNITY_ANDROID
        File.Copy(image, newFileLocation);
#else
        File.Move(image, newFileLocation);
#endif
        _availableCustomSkyboxPaths.Add(newFileLocation);
        _availableCustomSkyboxNames.Add(imageName);
        SaveSkyboxThumbnail(imageName, newFileLocation, token).Forget();
        return true;
    }

    public static void DeleteSkybox(string skyboxName)
    {
        var thumbnail = $"{AssetManager.DataPath}{AssetManager.CustomSkyboxThumbnailsFolder}{skyboxName}.png";
        var skyboxPath = $"{AssetManager.DataPath}{AssetManager.CustomSkyboxFolder}{skyboxName}";
        if (_availableCustomSkyboxPaths.Contains(skyboxPath))
        {
            _availableCustomSkyboxPaths.Remove(skyboxPath);
        }
        if (_availableCustomSkyboxNames.Contains(skyboxName))
        {
            _availableCustomSkyboxNames.Remove(skyboxName);
        }
        if (_availableCustomSkyboxDepthPaths.Contains(skyboxPath))
        {
            _availableCustomSkyboxDepthPaths.Remove(skyboxPath);
        }
        if (_availableCustomSkyboxDepthNames.Contains(skyboxName))
        {
            _availableCustomSkyboxDepthNames.Remove(skyboxName);
        }
        if (File.Exists(thumbnail))
        {
            File.Delete(thumbnail);
        }
        if (File.Exists(skyboxPath))
        {
            File.Delete(skyboxPath);
        }
        DeleteEnvironmentSkyboxes(skyboxName).Forget();
    }

    public static bool RenameSkybox(string skyboxName, string newName)
    {
        var thumbnail = $"{AssetManager.DataPath}{AssetManager.CustomSkyboxThumbnailsFolder}{skyboxName}.png";
        var skyboxPath = $"{AssetManager.DataPath}{AssetManager.CustomSkyboxFolder}{skyboxName}";
        var newThumbnail = $"{AssetManager.DataPath}{AssetManager.CustomSkyboxThumbnailsFolder}{newName}.png";
        var newSkyboxPath = $"{AssetManager.DataPath}{AssetManager.CustomSkyboxFolder}{newName}";

        if (File.Exists(newThumbnail) || File.Exists(newSkyboxPath))
        {
            return false;
        }

        var index = _availableCustomSkyboxPaths.IndexOf(skyboxPath);
        if (index >= 0)
        {
            _availableCustomSkyboxPaths[index] = newSkyboxPath;
        }

        index = _availableCustomSkyboxNames.IndexOf(skyboxName);
        if (index >= 0)
        {
            _availableCustomSkyboxNames[index] = newName;
        }

        index = _availableCustomSkyboxDepthPaths.IndexOf(skyboxPath);
        if (index >= 0)
        {
            _availableCustomSkyboxDepthPaths[index] = newSkyboxPath;
        }

        index = _availableCustomSkyboxDepthNames.IndexOf(skyboxName);
        if (index >= 0)
        {
            _availableCustomSkyboxDepthNames[index] = newName;
        }

        if (File.Exists(thumbnail))
        {
            File.Move(thumbnail, newThumbnail);
        }
        if (File.Exists(skyboxPath))
        {
            File.Move(skyboxPath, newSkyboxPath);
        }
        RenameEnvironmentSkyboxes(skyboxName, newName, newSkyboxPath).Forget();
        return true;
    }

    private static async UniTaskVoid RenameEnvironmentSkyboxes(string oldName, string newName, string newPath)
    {
        var updated = false;
        foreach (var env in _availableCustomEnvironments)
        {
            if (!env.SkyboxName.Equals(oldName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            env.SetSkyboxName(newName);
            env.SetSkyboxPath(newPath);
            updated = true;
            await TrySaveEnvironment(env, true, true);
        }
        if (updated)
        {
            CustomEnvironmentsUpdated.Invoke();
        }
    }

    private static async UniTaskVoid DeleteEnvironmentSkyboxes(string skyboxName)
    {
        var updated = false;
        foreach (var env in _availableCustomEnvironments)
        {
            if (!env.SkyboxName.Equals(skyboxName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            env.SetSkyboxName(string.Empty);
            env.SetSkyboxPath(string.Empty);
            env.SetSkyboxSprite(null);
            updated = true;
            await TrySaveEnvironment(env, true, true);
        }
        if (updated)
        {
            CustomEnvironmentsUpdated.Invoke();
        }
    }

    public static CustomEnvironment CreateCustomEnvironment(string environmentName, int skyboxIndex, float brightness, EnvAssetReference gloves, EnvAssetReference targets, EnvAssetReference obstacles)
    {
        var skybox = _availableCustomSkyboxPaths[skyboxIndex];
        var skyboxName = skybox.Substring(skybox.LastIndexOf("/") + 1);
        var customEnvironment = new CustomEnvironment(environmentName, skyboxName, skybox, skyboxBrightness: brightness, gloves: gloves, targets: targets, obstacles: obstacles);
        ValidateEnvironment(customEnvironment);
        return customEnvironment;
    }
    public static CustomEnvironment CreateCustomEnvironment(string environmentName, string skyboxPath, float brightness, EnvAssetReference gloves, EnvAssetReference targets, EnvAssetReference obstacles)
    {
        var skyboxName = string.IsNullOrWhiteSpace(skyboxPath) ? string.Empty : skyboxPath.Substring(skyboxPath.LastIndexOf("/") + 1);
        var customEnvironment = new CustomEnvironment(environmentName, skyboxName, skyboxPath, skyboxBrightness: brightness, gloves: gloves, targets: targets, obstacles: obstacles);
        ValidateEnvironment(customEnvironment);
        return customEnvironment;
    }

    public static async UniTask<bool> TrySaveEnvironment(CustomEnvironment customEnvironment, bool overwriteDuplicates, bool updating)
    {
        var path = $"{AssetManager.EnvironmentsPath}{customEnvironment.EnvironmentName}{EnvironmentExtension}";

        if (File.Exists(path))
        {
            if (!overwriteDuplicates)
            {
                return false;
            }
        }
        else if (!updating)
        {
            EnvironmentControlManager.Instance.AddCustomEnvironment(customEnvironment, path);
        }
        using var streamWriter = File.CreateText(path);
        var json = JsonUtility.ToJson(customEnvironment);
        var writingTask = streamWriter.WriteAsync(json);

        await writingTask;

        streamWriter.Close();
        return true;
    }
    public static bool TryDeleteEnvironment(CustomEnvironment customEnvironment)
    {
        var path = $"{AssetManager.EnvironmentsPath}{customEnvironment.EnvironmentName}{EnvironmentExtension}";

        if (!File.Exists(path))
        {
            return false;
        }
        EnvironmentControlManager.Instance.RemoveCustomEnvironment(customEnvironment, path);
        File.Delete(path);

        return true;
    }
    public static bool TryDeleteEnvironment(string environmentName)
    {
        var path = $"{AssetManager.EnvironmentsPath}{environmentName}{EnvironmentExtension}";

        if (!File.Exists(path))
        {
            return false;
        }
        EnvironmentControlManager.Instance.RemoveCustomEnvironment(environmentName, path);
        File.Delete(path);

        return true;
    }

    public static async UniTask LoadCustomEnvironments()
    {
        _availableCustomEnvironments.Clear();
        foreach (var customEnvironment in _customEnvironments)
        {
            var environment = await LoadCustomEnvironment(customEnvironment);
            if (environment == null)
            {
                continue;
            }
            _availableCustomEnvironments.Add(environment);
        }
        CustomEnvironmentsUpdated.Invoke();
    }

    public static async UniTask<CustomEnvironment> LoadCustomEnvironment(string customEnvironment)
    {
        if (!File.Exists(customEnvironment))
        {
            Debug.LogWarning($"Failed to load environment at {customEnvironment} as it is missing.");
            return null;
        }
        var json = await File.ReadAllTextAsync(customEnvironment);
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning($"Failed to load environment at {customEnvironment} as it is empty.");
            return null;
        }
        var environment = JsonUtility.FromJson<CustomEnvironment>(json);
        if (environment == null)
        {
            Debug.LogWarning($"Failed to load environment at {customEnvironment} as it is failed to be read.");
            return null;
        }
        ValidateEnvironment(environment);
        return environment;
    }

    public static void AddNewEnvironment(CustomEnvironment customEnvironment)
    {
        if (_availableCustomEnvironments == null)
        {
            _availableCustomEnvironments = new List<CustomEnvironment>();
        }
        if (!_availableCustomEnvironments.Contains(customEnvironment))
        {
            _availableCustomEnvironments.Add(customEnvironment);
        }
        CustomEnvironmentsUpdated.Invoke();
    }

    public static CustomEnvironment GetCustomEnvironment(int index)
    {
        if (index < 1 && index >= _customEnvironments.Count)
        {
            return null;
        }
        return _availableCustomEnvironments[index];
    }

    public static string GetSkyboxPath(int index)
    {
        if (index < 1 && index >= _availableCustomSkyboxPaths.Count)
        {
            return null;
        }
        return _availableCustomSkyboxPaths[index];
    }
    public static string GetSkyboxName(int index)
    {
        if (index < 1 && index >= _availableCustomSkyboxNames.Count)
        {
            return null;
        }
        return _availableCustomSkyboxNames[index];
    }
    public static string GetSkyboxDepthPath(int index)
    {
        if (index < 1 && index >= _availableCustomSkyboxDepthPaths.Count)
        {
            return null;
        }
        return _availableCustomSkyboxDepthPaths[index];
    }
    public static string GetSkyboxDepthName(int index)
    {
        if (index < 1 && index >= _availableCustomSkyboxDepthNames.Count)
        {
            return null;
        }
        return _availableCustomSkyboxDepthPaths[index];
    }

    public static string GetDownloadsImagePath(int index)
    {
        if (_imagesInDownloads == null || index < 1 && index >= _imagesInDownloads.Count)
        {
            return null;
        }
        return _imagesInDownloads[index];
    }
    public static string GetDownloadsImageName(int index)
    {
        if (_imagesInDownloads == null || index < 1 && index >= _imagesInDownloads.Count)
        {
            return null;
        }
        var name = _imagesInDownloads[index].Substring(_imagesInDownloads[index].LastIndexOf("/") + 1);
        return name;
    }
    public static void ClearCustomEnvironmentInfo()
    {
        _availableCustomSkyboxPaths?.Clear();
        _availableCustomSkyboxNames?.Clear();
        _availableCustomSkyboxDepthPaths?.Clear();
        _availableCustomSkyboxDepthNames?.Clear();
        if (_imagesInDownloads != null)
        {
            CollectionPool<List<string>, string>.Release(_imagesInDownloads);
        }
        _customEnvironments?.Clear();
        _availableCustomEnvironments?.Clear();
        _skyboxThumbnails?.Clear();
    }

    public static void ValidateEnvironment(CustomEnvironment customEnvironment)
    {
        customEnvironment.isValid =
            (string.IsNullOrWhiteSpace(customEnvironment.SkyboxPath) || File.Exists(customEnvironment.SkyboxPath)) &&
            (string.IsNullOrWhiteSpace(customEnvironment.SkyboxDepthPath) || File.Exists(customEnvironment.SkyboxDepthPath)) &&
            (string.IsNullOrWhiteSpace(customEnvironment.MeshPath) || File.Exists(customEnvironment.MeshPath));
    }

    private static bool TryGetThumbnail(string thumbnailKey, out Sprite thumbnail)
    {
        var hasThumbnail = _skyboxThumbnails.TryGetValue(thumbnailKey, out thumbnail);
        return hasThumbnail && thumbnail != null;
    }
}
