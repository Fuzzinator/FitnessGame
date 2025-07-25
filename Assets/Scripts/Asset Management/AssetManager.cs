using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

#if UNITY_ANDROID
using UnityEngine.Android;
using static UnityEngine.XR.Hands.XRHandSubsystemDescriptor;
using System.Security.AccessControl;
using UnityEngine.InputSystem;
using System.Runtime.InteropServices.ComTypes;
using static SongInfo;
//using System.Net.NetworkInformation;
#endif
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
#endif

public class AssetManager : MonoBehaviour
{
    private static string _dataPath;
    private static string _customSkyboxesPath;

    public static string DataPath
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_dataPath))
            {
                _dataPath = GetDataPath();
            }

            return _dataPath;
        }
    }

    #region Const Strings
#if UNITY_EDITOR
    private const string PAUSEINEDITOR = "Pause In Editor";
    private const string SONGSFOLDER = "/LocalCustomSongs/Songs/";
    private const string PLAYLISTSFOLDER = "/LocalCustomSongs/Playlists/";
    public const string CustomSkyboxFolder = "/LocalCustomSkyboxes/";
    public const string CustomSkyboxThumbnailsFolder = "/LocalCustomSkyboxThumbnails/";
    private const string CustomEnvironmentFolder = "/LocalCustomEnvironments/";
#else
    private const string SONGSFOLDER = "/Resources/Songs/";
    private const string PLAYLISTSFOLDER = "/Resources/Playlists/";    
    public const string CustomSkyboxFolder = "/Resources/LocalCustomSkyboxes/";
    public const string CustomSkyboxThumbnailsFolder = "/Resources/LocalCustomSkyboxThumbnails/";
    private const string CustomEnvironmentFolder = "/Resources/LocalCustomEnvironments/";

#if UNITY_ANDROID
    private const string ANDROIDPATHSTART = "file://";
    private const string DownloadsFolder = "/sdcard/Download/";
    private const string RootFolder = "/sdcard";
#endif


#endif

    public static readonly string SongsPath = $"{DataPath}{SONGSFOLDER}";
    public static readonly string PlaylistsPath = $"{DataPath}{PLAYLISTSFOLDER}";
    public static readonly string SkyboxesPath = $"{DataPath}{CustomSkyboxFolder}";
    public static readonly string SkyboxThumbnailPath = $"{DataPath}{CustomSkyboxThumbnailsFolder}";
    public static readonly string EnvironmentsPath = $"{DataPath}{CustomEnvironmentFolder}";

    private const string PLAYLISTEXTENSION = ".txt";
    private const string JPGEXTENSION = ".jpg";

    public const string LOCALSONGSFOLDER = "Assets/Music/Songs/";
    public const string LOCALPLAYLISTSFOLDER = "Assets/Music/Playlists/";

    private const string SONGINFONAME = "Info.txt";
    private const string ALTSONGINFONAME = "Info.dat";
    private const string ConvertedAltSongInfoName = "ConvertedInfo.dat";

    private const string LocalSongsFolderName = "/Auto-Convert Songs";

    #endregion

    #region Const ints

    public const int TEXTURESIZE = 256;

    #endregion

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    private static Guid DownloadsFolder = new Guid("374DE290-123F-4565-9164-39C4925E467B");

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path);

#endif

    public static string DownloadsPath()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        if (System.Environment.OSVersion.Version.Major < 6) throw new NotSupportedException();

        IntPtr pathPtr = IntPtr.Zero;

        try
        {
            SHGetKnownFolderPath(ref DownloadsFolder, 0, IntPtr.Zero, out pathPtr);
            return Marshal.PtrToStringUni(pathPtr);
        }
        finally
        {
            Marshal.FreeCoTaskMem(pathPtr);
        }
#elif UNITY_ANDROID
        return DownloadsFolder;
#endif
    }

    private static string GetDataPath()
    {
        return
#if UNITY_EDITOR
            Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/') + 1);
#elif UNITY_ANDROID
            Application.persistentDataPath;
#elif UNITY_STANDALONE_WIN
            Application.dataPath;
#endif
    }

    private static string GetAutoConvertSongsPath()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return $"{Application.dataPath}{LocalSongsFolderName}";
#elif UNITY_ANDROID
        return $"{DataPath}{LocalSongsFolderName}";
#endif
    }

    public static bool CheckPermissions()
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
#else

        return true;
#endif
    }


    /*public static async UniTask<bool> CheckPermissionsAsync()
    {
#if UNITY_ANDROID
        var readPermission = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead);
        if (!readPermission)
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
            await UniTask.NextFrame();
            await UniTask.WaitWhile(() => !Application.isFocused);
            readPermission = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead);
        }

        var writePermission = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite);
        if (!writePermission)
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            await UniTask.NextFrame();
            await UniTask.WaitWhile(() => !Application.isFocused);
            writePermission = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead);
        }

        return readPermission && writePermission;
#else

        return true;
#endif
    }*/

    public static async UniTask<AudioClip> LoadCustomSong(string path, CancellationToken cancellationToken, AudioType audioType, bool deleteFolder, bool stream = true)
    {
        try
        {
            var uwr = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
            ((DownloadHandlerAudioClip)uwr.downloadHandler).streamAudio = stream;
            var request = uwr.SendWebRequest();
            await request.ToUniTask(cancellationToken: cancellationToken);

            if (uwr.isDone && uwr.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip;
                if (ErrorReporter.Instance != null)
                {
                    ErrorReporter.SetSuppressed(true, true);
                    clip = DownloadHandlerAudioClip.GetContent(uwr);
                    ErrorReporter.SetSuppressed(false);
                }
                else
                {
                    clip = DownloadHandlerAudioClip.GetContent(uwr);
                }

                if (clip.length == 0)
                {
                    uwr.Dispose();
                    if (deleteFolder)
                    {
                        var directory = Path.GetDirectoryName(path);
                        Directory.Delete(directory, true);
                    }
                }
                clip.name = Path.GetFileName(path);
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
        catch (Exception e)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                return null;
            }
            Debug.LogError($"failed to get audio clip\n {e.Message}, {e.StackTrace}");
            return null;
        }
    }

    public static async UniTask<AudioClip> LoadCustomSong(string parentDirectory, SongInfo info,
        CancellationToken cancellationToken, bool stream)
    {
#if !UNITY_EDITOR
        if (!await PermissionsRequester.Instance.HasReadAndWritePermissions())
        {
            return null;
        }
#endif
#if UNITY_EDITOR
        var path = $"{SongsPath}{parentDirectory}/{info.SongFilename}";
#else
#if UNITY_ANDROID
            var path =
            $"{ANDROIDPATHSTART}{SongsPath}{parentDirectory}/{info.SongFilename}";
#elif UNITY_STANDALONE_WIN
            var path = $"{SongsPath}{parentDirectory}/{info.SongFilename}";
#endif

#endif

        return await LoadCustomSong(path, cancellationToken, AudioType.OGGVORBIS, true, stream);
    }

    public static async UniTask<AudioClipRequest> LoadBuiltInSong(SongInfo item, CancellationToken cancellationToken)
    {
        AsyncOperationHandle requestHandle = new AsyncOperationHandle();
        try
        {
            var fileName = item.SongFilename;
            var request = Addressables.LoadAssetAsync<AudioClip>($"{LOCALSONGSFOLDER}{item.fileLocation}/{fileName}");
            requestHandle = request;
            await request.ToUniTask(cancellationToken: cancellationToken);

            var clip = request.Result;
            if (clip == null)
            {
                Addressables.Release(request);
                Debug.LogError("Failed to load local resource file");
                return new AudioClipRequest();
            }

            clip.name = item.SongName;
            return new AudioClipRequest(clip, request);
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
            if (requestHandle.IsValid())
            {
                Addressables.Release(requestHandle);
            }
            return new AudioClipRequest();
        }
    }

    public static async UniTask<Texture2D> LoadCustomSongImage(string parentDirectory, SongInfo info,
        CancellationToken cancellationToken)
    {
#if !UNITY_EDITOR
        if (!await PermissionsRequester.Instance.HasReadAndWritePermissions())
        {
            return null;
        }
#endif
        var path = $"{SongsPath}{parentDirectory}/{info.ImageFilename}";
        return await LoadImageFromPath(path, cancellationToken);
    }

    public static async UniTask<Texture2D> LoadImageFromPath(string path, CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(path))
            {
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
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
            return null;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Debug.LogError($"Failed to load image\n {e.Message}");
            return null;
        }
    }

    public static async UniTask<Texture2DRequest> LoadBuiltInSongImage(SongInfo item, CancellationToken cancellationToken)
    {
        var fileName = item.ImageFilename;
        var fileLocation = $"{LOCALSONGSFOLDER}{item.fileLocation}/{fileName}";

        if (!AddressableExists(fileLocation))
        {
            return new Texture2DRequest();
        }
        AsyncOperationHandle<Texture2D> requestHandle = new AsyncOperationHandle<Texture2D>();
        try
        {
            requestHandle = Addressables.LoadAssetAsync<Texture2D>(fileLocation);

            await requestHandle.ToUniTask(cancellationToken: cancellationToken);

            var texture = requestHandle.Result;
            if (texture == null)
            {
                Debug.LogError($"Failed to load local resource file for {item.SongName}");
                if (requestHandle.IsValid())
                {
                    Addressables.Release(requestHandle);
                }
                return new Texture2DRequest();
            }

            texture.name = item.SongName;
            return new Texture2DRequest(texture, requestHandle);
        }
        catch (Exception e)
        {
            if (e is not OperationCanceledException or InvalidKeyException)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
            }

            if (requestHandle.IsValid())
            {
                Addressables.Release(requestHandle);
            }
            return new Texture2DRequest();
        }

    }

    private static bool AddressableExists(object key)
    {
        foreach (var item in Addressables.ResourceLocators)
        {
            IList<IResourceLocation> locs;
            if (item.Locate(key, null, out locs)) return true;
        }
        return false;
    }


    public static async UniTask<Texture2DRequest> LoadBuiltInPlaylistImage(string playlistName,
        CancellationToken cancellationToken)
    {
        AsyncOperationHandle requestHandle = new AsyncOperationHandle();
        try
        {
            var request = Addressables.LoadAssetAsync<Texture2D>($"{LOCALPLAYLISTSFOLDER}{playlistName}.jpg");
            requestHandle = request;
            await request.ToUniTask(cancellationToken: cancellationToken);

            var texture = request.Result;
            if (texture == null)
            {
                Debug.LogError($"Failed to load local resource file for {playlistName}");
                if (requestHandle.IsValid())
                {
                    Addressables.Release(requestHandle);
                }
                return new Texture2DRequest();
            }

            texture.name = playlistName;
            return new Texture2DRequest(texture, requestHandle);
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
            if (requestHandle.IsValid())
            {
                Addressables.Release(requestHandle);
            }
            return new Texture2DRequest();
        }
    }

    public static async UniTask GetCustomPlaylists(Action<Playlist> playlistLoaded, CancellationToken cancellationToken)
    {
        if (!await PermissionsRequester.Instance.HasReadAndWritePermissions())
        {
            return;
        }

        if (!Directory.Exists(PlaylistsPath))
        {
            Directory.CreateDirectory(PlaylistsPath);
        }

        var info = new DirectoryInfo(PlaylistsPath);
        var files = info.GetFiles();
        var readSuccess = false;
        foreach (var file in files)
        {
            try
            {
                if (file.Extension == PLAYLISTEXTENSION)
                {
                    var streamReader = new StreamReader(file.FullName);
                    var playlistAsJson = await streamReader.ReadToEndAsync();
                    var playlist = JsonUtility.FromJson<Playlist>(playlistAsJson);
                    streamReader.Close();


                    var imagePath = $"{PlaylistsPath}{playlist.PlaylistName}.jpg";
                    if (!File.Exists(imagePath))
                    {
                        //Debug.LogWarning($"No image found at\"{imagePath}\"");
                    }
                    else
                    {
                        var bytes = await File.ReadAllBytesAsync(imagePath, cancellationToken);
                        if (bytes == null || bytes.Length == 0)
                        {
                            Debug.LogError($"Failed to load image at \"{imagePath}\"");
                        }
                        else
                        {
                            var texture = new Texture2D(2, 2);
                            texture.LoadImage(bytes);
                            playlist.SetIcon(texture);
                        }
                    }

                    #region Upgrading Playlists

                    var shouldSave = false;
                    if (string.IsNullOrWhiteSpace(playlist.Version))
                    {
                        playlist.UpgradePlaylistSoSongsAreOverrides();
                        shouldSave = true;
                    }

                    if (string.IsNullOrWhiteSpace(playlist.GUID))
                    {
                        playlist.UpgradePlaylistAddGuid();
                        shouldSave = true;
                    }

                    if (shouldSave)
                    {
                        await SavePlaylist(playlist, file.FullName);
                    }

                    #endregion

                    readSuccess = true;

                    playlist.isValid = await PlaylistValidator.IsValid(playlist);
                    playlistLoaded?.Invoke(playlist);
                    await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
                }
            }
            catch (Exception e)
            {
                ErrorReporter.SetSuppressed(true);
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
                ErrorReporter.SetSuppressed(false);

                if (file.Extension == PLAYLISTEXTENSION)
                {
                    var visuals = new Notification.NotificationVisuals($"Failed to load playlist: {file.Name}. It may have been corrupted.", "Reading Playlist Failed", "Delete Playlist", "Ignore");
                    NotificationManager.RequestNotification(visuals, () => File.Delete(file.FullName));
                }
            }
        }
    }

    public static async UniTask<List<AsyncOperationHandle>> GetBuiltInPlaylists(string label, Action<Playlist> playlistLoaded,
        CancellationToken cancellationToken)
    {
        List<AsyncOperationHandle> asyncHandles = null;
        var request = Addressables.LoadAssetsAsync<TextAsset>(label, async asset =>
        {
            if (asset == null)
            {
                return;
            }

            var playlist = JsonUtility.FromJson<Playlist>(asset.text);

            if (playlist == null)
            {
                Debug.LogWarning($"Playlist of name {label} was null.");
                return;
            }

            playlist.isValid = await PlaylistValidator.IsValid(playlist); //This is a temporary solution.
            var textureRequest = await LoadBuiltInPlaylistImage(playlist.PlaylistName, cancellationToken);
            if (asyncHandles == null)
            {
                asyncHandles = new List<AsyncOperationHandle>();
            }
            if (textureRequest.IsValid)
            {
                asyncHandles.Add(textureRequest.OperationHandle);
            }
            playlist.SetIcon(textureRequest.Texture);

            playlistLoaded?.Invoke(playlist);
            await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
        });
        if (request.IsValid())
        {
            Addressables.Release(request);
        }
        await request;
        return asyncHandles;
    }

    public static async UniTask GetBuiltInSongs(AssetLabelReference label, Action<SongInfo> songLoaded)
    {
        var request = Addressables.LoadAssetsAsync<TextAsset>(label, asset =>
        {
            if (asset == null)
            {
                return;
            }

            var item = JsonUtility.FromJson<SongInfo>(asset.text);
            item.isCustomSong = false;

            songLoaded?.Invoke(item);
        });
        if (request.IsValid())
        {
            Addressables.Release(request);
        }
        await request;
    }

    public static async UniTask GetCustomSongs(Action<SongInfo> songLoaded, CancellationTokenSource cancellationSource, bool skipUpdating = false)
    {
#if !UNITY_EDITOR
        if (!await PermissionsRequester.Instance.HasReadAndWritePermissions())
        {
            return;
        }
#endif

        if (!Directory.Exists(SongsPath))
        {
            Directory.CreateDirectory(SongsPath);
        }

        var directories = Directory.GetDirectories(SongsPath);

        var songFailed = false;
        for (int i = 0; i < directories.Length; i++)
        {
            var dir = directories[i];
            /* string folderName = null;
             if(dir.Contains("/"))
             {
                 folderName = dir.Substring(dir.LastIndexOf("/") + 1);
             }
             else if (dir.Contains("\\"))
             {
                 folderName = dir.Substring(dir.LastIndexOf("\\") + 1);
             }
             if(folderName != null && folderName.ContainsIllegalCharacters())
             {*/
#if UNITY_ANDROID
            if (dir.Contains("+"))
            {
                NotifyWontLoadPlusFolders(dir);
                //dir = await CleanPlusFolders(dir);
                continue;
            }
#endif
            SongInfo item = null;
            try
            {
                item = await GetSingleCustomSong(dir, cancellationSource.Token, skipUpdating: skipUpdating);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{ex.Message}-{ex.StackTrace}");
            }

            if (item != null)
            {
                if (!string.IsNullOrWhiteSpace(item.SongID))
                {
                    if (!item.AutoConverted)
                    {
                        CustomSongsManager.AddToRecord(item.SongID);
                    }
                }

                songLoaded?.Invoke(item);
            }
            else
            {
                songFailed = true;
            }
        }

        if (songFailed)
        {
            var visuals = new Notification.NotificationVisuals("Some songs failed to load and may have been corrupted. Corrupted songs have been automatically removed.", "Issue Reading Songs", "Okay");
            NotificationManager.RequestNotification(visuals);
        }
    }

    public static async UniTask<SongInfo> TryGetSingleCustomSong(string fileLocation, CancellationToken token, string songID = null, float songScore = -1f, bool skipUpdating = false)
    {
#if !UNITY_EDITOR
        if (!await PermissionsRequester.Instance.HasReadAndWritePermissions())
        {
            return null;
        }
#endif
        var path = $"{SongsPath}{fileLocation}";
        if (!Directory.Exists(path))
        {
            return null;
            //Directory.CreateDirectory(path);
        }
        /* if (fileLocation.ContainsIllegalCharacters())
         {
             var cleanString = $"{SongsPath}{fileLocation.RemoveIllegalIOCharacters()}";
             Directory.Move(path, cleanString);
             path = cleanString;
         }*/

#if UNITY_ANDROID
        if (path.Contains("+"))
        {
            NotifyWontLoadPlusFolders(path);
            //path = await CleanPlusFolders(path);
            return null;
        }
#endif
        SongInfo song = null;
        try
        {
            song = await GetSingleCustomSong(path, token, songID, songScore, skipUpdating);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }

        if (song == null)
        {
            var hasBeenPromptedToHelp = SettingsManager.GetSetting("PlayerHasBeenAskedToSendErrorLogs", false);

            if (hasBeenPromptedToHelp)
            {
                var visuals = new Notification.NotificationVisuals("The song has failed to load and may be corrupted. Corrupted songs are automatically removed and have to be redownloaded.", "Failed Reading Song", "Okay");
                NotificationManager.RequestNotification(visuals);
            }
        }

        return song;
    }

    public static async UniTask<FileInfo> ConvertFromBeatSage(SongInfo songInfo, string fileLocation, bool autoSave,
        List<FileInfo> easyFiles, List<FileInfo> normalFiles, List<FileInfo> hardFiles, List<FileInfo> expertFiles, List<FileInfo> expertPlusFiles,
        CancellationToken token)
    {
        var path = $"{SongsPath}{fileLocation}";
        var info = new DirectoryInfo(path);
        var files = info.GetFiles();
        FileInfo infoFile = null;

        if (files.Length == 0)
        {
            info.Delete(true);
        }
        foreach (var file in files)
        {
            if (file == null)
            {
                continue;
            }

            if (string.Equals(file.Name, SONGINFONAME, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(file.Name, ALTSONGINFONAME, StringComparison.InvariantCultureIgnoreCase))
            {
                songInfo.ConvertFromBeatSage();
                infoFile = file;

                if (autoSave)
                {
                    using (var streamWriter = new StreamWriter(file.FullName))
                    {
                        await streamWriter.WriteAsync(JsonUtility.ToJson(songInfo));
                        streamWriter.Close();
                    }
                }

                continue;
            }
            else if (file.Extension.Equals(".dat", StringComparison.InvariantCultureIgnoreCase) && !file.Extension.Contains("-Expert"))
            {
                if (file.Name.Contains("ExpertPlus"))
                {
                    expertPlusFiles.Add(file);
                }
                else if (file.Name.Contains("Expert"))
                {
                    expertFiles.Add(file);
                }
                else if (file.Name.Contains("Hard"))
                {
                    hardFiles.Add(file);
                }
                else if (file.Name.Contains("Normal"))
                {
                    normalFiles.Add(file);
                }
                else if (file.Name.Contains("Easy"))
                {
                    easyFiles.Add(file);
                }
            }
        }
        while (easyFiles.Count > 0)
        {
            var file = easyFiles[0];
            easyFiles.RemoveAt(0);
            file.Delete();
        }
        foreach (var file in normalFiles)
        {
            var newName = file.Name.Replace("Normal", "Easy");
            file.MoveTo($"{info.FullName}/{newName}");
        }
        normalFiles.Clear();

        foreach (var file in hardFiles)
        {
            var newName = file.Name.Replace("Hard", "Normal");
            file.MoveTo($"{info.FullName}/{newName}");
        }
        hardFiles.Clear();

        foreach (var file in expertFiles)
        {
            var newName = file.Name.Replace("Expert", "Hard");
            file.MoveTo($"{info.FullName}/{newName}");
        }
        expertFiles.Clear();

        foreach (var file in expertPlusFiles)
        {
            var newName = file.Name.Replace("ExpertPlus", "Expert");
            file.MoveTo($"{info.FullName}/{newName}");
        }
        expertPlusFiles.Clear();

        return infoFile;
    }

    public static async UniTask ConvertToLocal(string fileLocation, string localPath, CancellationToken token)
    {
        var path = $"{SongsPath}{fileLocation}";
        var info = new DirectoryInfo(path);
        var files = info.GetFiles();
        var newFolder = string.Empty;
        if (files.Length == 0)
        {
            info.Delete(true);
        }
        foreach (var file in files)
        {
            if (file == null)
            {
                return;
            }

            if (string.Equals(file.Name, SONGINFONAME, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(file.Name, ALTSONGINFONAME, StringComparison.InvariantCultureIgnoreCase))
            {
                var streamReader = new StreamReader(file.FullName);
                var result = await streamReader.ReadToEndAsync().AsUniTask()
                    .AttachExternalCancellation(token);

                SongInfo item = null;
                try
                {
                    item = JsonUtility.FromJson<SongInfo>(result);
                }
                catch (Exception ex)
                {
                    ErrorReporter.SetSuppressed(true);
                    Debug.LogError($"Failed to load song at {fileLocation}___{ex}");
                    ErrorReporter.SetSuppressed(false);
                    streamReader.Close();
                    break;
                }

                streamReader.Close();

                item.ConvertToLocal();
                newFolder = $"{localPath}/{item.fileLocation}";
                Directory.CreateDirectory(newFolder);
                /*using (var streamWriter = new StreamWriter($"{newFolder}/info.txt"))
                {
                    await streamWriter.WriteAsync(JsonUtility.ToJson(item));
                    streamWriter.Close();
                }*/
            }
        }
        foreach (var file in files)
        {
            if (file.Extension.Equals(".dat", StringComparison.InvariantCultureIgnoreCase))
            {
                file.MoveTo($"{newFolder}/{file.Name.Replace(".dat", ".txt")}");
            }
            else
            {
                file.MoveTo($"{newFolder}/{file.Name}");
            }

        }
    }

#if UNITY_ANDROID
    private static void NotifyWontLoadPlusFolders(string fullPath)
    {
        //var separator = fullPath.Contains('/') ? "/" : "\\";
        //var cleanString = fullPath.Substring(fullPath.LastIndexOf(separator) + 1);
        var visuals = new Notification.NotificationVisuals("Songs in folders containing the character \"+\" cannot be read. Please rename your folders and re-add them.", "Failed To Read Some Songs", popUp: true, autoTimeOutTime: 7.5f);
        NotificationManager.RequestNotification(visuals);
    }
#endif

    public static async UniTask<SongInfo> GetSingleCustomSong(string fileLocation, CancellationToken token, string songID = null, float songScore = -1f, bool skipUpdating = false)
    {
        var fileCorrupted = false;

        var info = new DirectoryInfo(fileLocation);
        var creationDate = info.CreationTime;
        var files = info.GetFiles();
        if (files.Length == 0)
        {
            info.Delete(true);
        }

        FileInfo readOnlyFile = null;
        FileInfo songFile = null;

        foreach (var file in files)
        {
            if (file == null)
            {
                return null;
            }

            if (string.Equals(file.Name, SONGINFONAME, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(file.Name, ALTSONGINFONAME, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(file.Name, ConvertedAltSongInfoName, StringComparison.InvariantCultureIgnoreCase))
            {

                if (file.IsReadOnly)
                {
                    readOnlyFile = file;
                    continue;
                }
                songFile = file;
                var songInfo = await TryProcessSong(fileLocation, creationDate, info, file, skipUpdating, songID, songScore, token);
                fileCorrupted = songInfo == null;
                if (fileCorrupted && readOnlyFile == null)
                {
                    DeleteCorruptedFile(info).Forget();
                }
                return songInfo;
            }
        }

        if (songFile == null && readOnlyFile != null)
        {
            var songInfo = await TryProcessSong(fileLocation, creationDate, info, readOnlyFile, skipUpdating, songID, songScore, token);
            fileCorrupted = songInfo == null;
            if (fileCorrupted && readOnlyFile == null)
            {
                DeleteCorruptedFile(info).Forget();
            }

            return songInfo;
        }


        if (fileCorrupted && readOnlyFile == null)
        {
            DeleteCorruptedFile(info).Forget();
        }
        return null;
    }

    private static async UniTask<SongInfo> TryProcessSong(string fileLocation, DateTime creationDate, DirectoryInfo dirInfo, FileInfo fileInfo,
                                                        bool skipUpdating, string songID, float songScore, CancellationToken token)
    {
        var streamReader = new StreamReader(fileInfo.FullName);
        var result = await streamReader.ReadToEndAsync().AsUniTask()
            .AttachExternalCancellation(token);

        SongInfo songInfo = null;
        try
        {
            songInfo = JsonUtility.FromJson<SongInfo>(result);
        }
        catch (Exception ex)
        {
            ErrorReporter.SetSuppressed(true);
            Debug.LogError($"Failed to load song at {fileLocation}___{ex}");
            ErrorReporter.SetSuppressed(false);
            streamReader.Close();
            return null;
        }

        streamReader.Close();
        if (songInfo == null)
        {
            Debug.LogWarning($"Failed to read song info in {dirInfo.Name}. It is likely corrupted");
            return null;
        }

        var updatedMaps = false;
        if (fileInfo.Directory != null && string.IsNullOrWhiteSpace(songInfo.fileLocation))
        {
            songInfo.fileLocation = fileInfo.Directory.Name;
            updatedMaps = true;
        }

        if (songInfo.SongLength < 1)
        {
            var songLength = await CustomSongsManager.TryGetSongLength(songInfo, token);
            songInfo.SongLength = songLength;
            updatedMaps = true;
            if (songLength == 0)
            {
                return null;
            }
        }

        if (skipUpdating)
        {
            return songInfo;
        }

        return await UpdateMap(songInfo, fileInfo, updatedMaps, songID, songScore, creationDate, token);
    }

    public static async UniTask<SongInfo> UpdateMap(SongInfo songInfo, FileInfo file, bool updatedMaps, string songID, float songScore, DateTime creationDate, CancellationToken token)
    {
        if (!songInfo.isCustomSong)
        {
            songInfo.isCustomSong = true;
            updatedMaps = true;
        }

        Sprite image = null;
        if (file.IsReadOnly)
        {
            songInfo.UseConvertedFileNames = true;
            image = await songInfo.LoadImage(token);
            if (image == null)
            {
                songInfo.UseConvertedFileNames = false;
            }
        }

        if (image == null)
        {
            image = await songInfo.LoadImage(token);
        }

        if (image != null && image.texture.width != TEXTURESIZE)
        {
            await UniTask.DelayFrame(1, cancellationToken: token);
            var tex = image.texture.ScaleTexture(TEXTURESIZE, TEXTURESIZE, TextureFormat.RGB24);
            var bytes = tex.EncodeToJPG();
            if (file.IsReadOnly)
            {
                songInfo.UseConvertedFileNames = true;
            }
            try
            {
                await File.WriteAllBytesAsync($"{file.DirectoryName}/{songInfo.ImageFilename}", bytes, token);
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException)
                {
                    songInfo.UseConvertedFileNames = true;
                    await File.WriteAllBytesAsync($"{file.DirectoryName}/{songInfo.ImageFilename}", bytes, token);
                }
            }
            songInfo.SetImage(tex);
        }

        var updated = await songInfo.UpdateDifficultySets(token);

        if (!updated.Success)
        {
            var visuals = new Notification.NotificationVisuals(
                $"\"{songInfo.SongName}\" was unable to be read and will not be imported.",
                $"Failed To Load Song", autoTimeOutTime: 2.5f);
            NotificationManager.RequestNotification(visuals);
            return null;
        }

        if (updated.MadeChange)
        {
            updatedMaps = true;
        }
        if (!string.IsNullOrWhiteSpace(songID) && string.IsNullOrWhiteSpace(songInfo.SongID))
        {
            songInfo.SetSongID(songID);
            if (!songInfo.AutoConverted)
            {
                CustomSongsManager.AddToRecord(songID);
            }
            updatedMaps = true;
        }
        if (songScore >= 0 && songInfo.SongScore <= 0)
        {
            songInfo.SetSongScore(songScore);
            updatedMaps = true;
        }

        if (updatedMaps)
        {
            await UniTask.DelayFrame(2, cancellationToken: token);

            string writePath;
            if (file.IsReadOnly)
            {
                var fileLocation = file.DirectoryName;
                writePath = $"{fileLocation}/{ConvertedAltSongInfoName}";
            }
            else
            {
                writePath = file.FullName;
            }

            using (var streamWriter = new StreamWriter(writePath))
            {
                await streamWriter.WriteAsync(JsonUtility.ToJson(songInfo));
                streamWriter.Close();
            }
        }
        songInfo.DownloadedDate = creationDate;
        return songInfo;
    }

    private static async UniTaskVoid DeleteCorruptedFile(DirectoryInfo dirInfo)
    {
        await UniTask.DelayFrame(1);
        dirInfo.Delete(true);
    }

    public static bool TryDeleteCustomSong(SongInfo info)
    {
        if (info.UseConvertedFileNames)
        {
            var visuals = new Notification.NotificationVisuals(
                $"Shadow BoXR lacks the necessary permissions to delete {info.SongName}. Songs added outside of Shadow BoXR have to be deleted the same way they were added.",
                "Unable to Delete Song",
                "Okay");
            NotificationManager.RequestNotification(visuals);
            return false;
        }
        var path = $"{SongsPath}{info.fileLocation}";
        DeleteCustomSong(path);
        return true;
    }

    public static void DeleteCustomSong(string path)
    {
        if (!Directory.Exists(path))
        {
            Debug.LogWarning("Invalid path cannot delete.");
            return;
        }

        Directory.Delete(path, true);
    }

    public static void DeletePlaylist(string playlistName)
    {
        var filePath = $"{PlaylistsPath}{playlistName}{PLAYLISTEXTENSION}";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        var imagePath = $"{PlaylistsPath}{playlistName}{JPGEXTENSION}";
        if (File.Exists(imagePath))
        {
            File.Delete(imagePath);
        }
    }

    public static void DeletePlaylistImage(string playlistName, CancellationToken token)
    {
        var path = $"{DataPath}/{PLAYLISTSFOLDER}";
        var imagePath = $"{path}{playlistName}{JPGEXTENSION}";
        if (File.Exists(imagePath))
        {
            File.Delete(imagePath);
        }
    }

    private static async UniTask SavePlaylist(Playlist playlist, string fullPath)
    {
        using var streamWriter = File.CreateText(fullPath);
        var json = JsonUtility.ToJson(playlist);
        var writingTask = streamWriter.WriteAsync(json);

        await writingTask;

        streamWriter.Close();
    }

    public static void GetAssetPathsFromAutoConvert(string[] extensions, List<string> listOfFiles)
    {
        var rootFolder = GetAutoConvertSongsPath();
        if (!Directory.Exists(rootFolder))
        {
            Directory.CreateDirectory(rootFolder);
            return;
        }
        EnumerateDirectory(new DirectoryInfo(rootFolder), extensions, listOfFiles);
    }

    public static void GetAssetPathsFromDownloads(string[] extensions, List<string> listOfFiles)
    {
        var path = DownloadsPath();
        if (!Directory.Exists(path))
        {
            return;
        }
        EnumerateDirectory(new DirectoryInfo(path), extensions, listOfFiles);
    }

    private static void EnumerateDirectory(DirectoryInfo info, string[] extensions, List<string> listOfFiles)
    {
        var files = info.EnumerateFiles();
        foreach (var file in files)
        {
            if (file == null)
            {
                continue;
            }
            foreach (var extension in extensions)
            {

                if (string.Equals(file.Extension, extension, StringComparison.InvariantCultureIgnoreCase))
                {
                    var fullName = file.FullName.Replace('\\', '/');
                    if (!listOfFiles.Contains(fullName) && !listOfFiles.Contains(fullName))
                    {
                        listOfFiles.Add(fullName);
                    }
                }
            }
        }

        var directories = info.EnumerateDirectories();
        foreach (var directory in directories)
        {
            EnumerateDirectory(directory, extensions, listOfFiles);
        }
    }

    public static TagLib.File GetTagLib(SongInfo info, CancellationToken cancellationToken)
    {
        if (!CheckPermissions())
        {
            Debug.LogWarning("User did not give permissions cannot access custom files");
            return null;
        }
#if UNITY_EDITOR
        var path = $"{SongsPath}{info.fileLocation}/{info.SongFilename}";
#else
#if UNITY_ANDROID
            var path =
            $"{ANDROIDPATHSTART}{SongsPath}{info.fileLocation}/{info.SongFilename}";
#elif UNITY_STANDALONE_WIN
            var path = $"{SongsPath}{info.fileLocation}/{info.SongFilename}";
#endif

#endif

        return TagLib.File.Create(path);
    }

    public static void HideSong(string songID)
    {
        HiddenAssetManager.HideSong(songID);
    }

    public static void HidePlaylist(string guid)
    {
        HiddenAssetManager.HidePlaylist(guid);
    }
}

public struct AudioClipRequest
{
    public AudioClip AudioClip { get; private set; }
    public AsyncOperationHandle OperationHandle { get; private set; }
    public readonly bool IsValid { get; }

    public AudioClipRequest(AudioClip audioClip, AsyncOperationHandle operationHandle)
    {
        AudioClip = audioClip;
        OperationHandle = operationHandle;
        IsValid = true;
    }
}

public struct Texture2DRequest
{
    public Texture2D Texture { get; private set; }
    public AsyncOperationHandle OperationHandle { get; private set; }

    public readonly bool IsValid { get; }

    public Texture2DRequest(Texture2D texture, AsyncOperationHandle operationHandle)
    {
        Texture = texture;
        OperationHandle = operationHandle;
        IsValid = true;
    }
}
