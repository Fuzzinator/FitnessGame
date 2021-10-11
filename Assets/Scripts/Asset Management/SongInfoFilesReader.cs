using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif
using UnityEngine.Events;

public class SongInfoFilesReader : MonoBehaviour
{
    public static SongInfoFilesReader Instance { get; private set; }

    public List<SongInfo> availableSongs = new List<SongInfo>();

    [SerializeField]
    private AssetLabelReference _labelReference;

    [SerializeField]
    private UnityEvent _startSongsUpdate = new UnityEvent();
    [SerializeField]
    private UnityEvent _songsUpdated = new UnityEvent();

    #region Const Strings

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string ANDROIDPATHSTART = "file://";
    private const string SONGSFOLDER = "/Resources/Songs/";
#elif UNITY_EDITOR
    private const string UNITYEDITORLOCATION = "E:\\Projects\\FitnessGame\\LocalCustomSongs\\Songs";
#endif

    private const string SONGINFONAME = "Info.txt";
    private const string ALTSONGINFONAME = "Info.dat";

    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnEnable()
    {
        UpdateSongs();
    }

    private async void UpdateSongs()
    {
        _startSongsUpdate?.Invoke();
        await UpdateAvailableSongs();
    }

    private async UniTask UpdateAvailableSongs()
    {
        availableSongs.Clear();
        await GetBuiltInSongs();
        await GetCustomSongs();
        _songsUpdated?.Invoke();
    }

    private async UniTask GetBuiltInSongs()
    {
        await Addressables.LoadAssetsAsync<TextAsset>(_labelReference, asset =>
        {
            if (asset == null)
            {
                return;
            }

            var item = JsonUtility.FromJson<SongInfo>(asset.text);
            item.DifficultySets[0].RemoveExpertPlus();
            item.isCustomSong = false;

            availableSongs.Add(item);
        });
    }

    private async UniTask GetCustomSongs()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var path = $"{Application.persistentDataPath}{SONGSFOLDER}";
       
        /*if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }*/
#elif UNITY_EDITOR
        var path = UNITYEDITORLOCATION;
#endif
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var directories = Directory.GetDirectories(path);
        SongLoader songLoader = null;
        if (directories.Length > 0)
        {
            songLoader = new SongLoader();
        }

        //Parallel.ForEach(directories, dir => 
        foreach (var dir in directories)
        {
            var info = new DirectoryInfo(dir);
            var files = info.GetFiles();
            //await UniTask.WaitWhile(() => !Parallel.ForEach(files, async file => //This is commented out until I learn more about Parallel.Foreach
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
                    var reading = streamReader.ReadToEndAsync();
                    await reading;
                    var item = JsonUtility.FromJson<SongInfo>(reading.Result);
                    
                    streamReader.Close();
                    
                    item.DifficultySets[0].RemoveExpertPlus();
                    if (file.Directory != null)
                    {
                        item.fileLocation = file.Directory.Name;
                    }

                    item.isCustomSong = true;
                    if (item.SongLength < 1)
                    {
                        var clipRequest = TryGetSongLength(item, songLoader);
                        var task = clipRequest.AsTask();
                        await task;
                        item.SongLength = task.Result;
                        using (var streamWriter = new StreamWriter(file.FullName))
                        {
                            await streamWriter.WriteAsync(JsonUtility.ToJson(item));
                        }
                    }

                    availableSongs.Add(item);
                } 
            }//).IsCompleted);
        }//);
    }

    public async UniTask<float> TryGetSongLength(SongInfo info, SongLoader songLoader,
        bool customSong = true)
    {
        if (songLoader != null)
        {
            UniTask<AudioClip> clipRequest;
            if (customSong)
            {
                clipRequest = songLoader.LoadCustomSong(info.fileLocation, info);
            }
            else
            {
                clipRequest = songLoader.LoadBuiltInSong(info);
            }

            var task = clipRequest.AsTask();
            await task;
            var audioClip = task.Result;
            if (audioClip != null)
            {
                return audioClip.length;
            }
        }

        return 0;
    }
}