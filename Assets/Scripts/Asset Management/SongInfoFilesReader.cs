using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    private UnityEvent _songsUpdated = new UnityEvent();

    #region Const Strings

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string ANDROIDPATHSTART = "file://";
    private const string SONGSFOLDER = "/Resources/Songs/";
#elif UNITY_EDITOR
    private const string UNITYEDITORLOCATION = "E:\\Projects\\FitnessGame\\LocalCustomSongs\\Songs";
#endif

    private const string SONGINFONAME = "Info.txt";

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
        var path = $"{ANDROIDPATHSTART}{Application.persistentDataPath}{SONGSFOLDER}";
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
#elif UNITY_EDITOR
        var path = UNITYEDITORLOCATION;
#endif
        var directories = Directory.GetDirectories(path);
        foreach (var dir in directories)
        {
            var info = new DirectoryInfo(dir);
            var files = info.GetFiles();
            foreach (var file in files)
            {
                if (string.Equals(file.Name, SONGINFONAME, StringComparison.InvariantCultureIgnoreCase))
                {
                    var streamReader = new StreamReader(file.FullName);
                    var reading = streamReader.ReadToEndAsync();
                    await reading;
                    var item = JsonUtility.FromJson<SongInfo>(reading.Result);
                    item.DifficultySets[0].RemoveExpertPlus();
                    item.fileLocation = dir.Replace($"{path}\\", ""); 
                    item.isCustomSong = true;
                    availableSongs.Add(item);
                }
            }
        }
    }
}