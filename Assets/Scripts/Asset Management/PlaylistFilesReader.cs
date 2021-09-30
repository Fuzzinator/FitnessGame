using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.ResourceLocations;

public class PlaylistFilesReader : MonoBehaviour
{
    public static PlaylistFilesReader Instance { get; private set; }

    public List<Playlist> availablePlaylists = new List<Playlist>();

    [SerializeField]
    private AssetLabelReference _labelReference;
    [SerializeField]
    private UnityEvent _playlistsUpdated = new UnityEvent();
    
    #region Const Strings

#if UNITY_ANDROID  && !UNITY_EDITOR
    private const string ANDROIDPATHSTART = "file://";
    #elif UNITY_EDITOR
    private const string UNITYEDITORLOCATION = "E:\\Projects\\FitnessGame\\LocalCustomSongs\\Playlists";
#endif

    private const string PLAYLISTSFOLDER = "/Resources/Songs/";
    private const string PLAYLISTEXTENSION = ".txt";
    private const string PLAYLISTSKEY = "BuiltInSongs/Playlists";
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

    private void Start()
    {
        UpdateAvailablePlaylists();
    }

    private async UniTaskVoid UpdateAvailablePlaylists()
    {
        availablePlaylists.Clear();
        await GetBuiltInPlaylists();
        await GetCustomPlaylists();
        _playlistsUpdated?.Invoke();
        //availablePlaylists = GetCustomPlaylists();
    }

    private async UniTask GetBuiltInPlaylists()
    {
        await Addressables.LoadAssetsAsync<TextAsset>(_labelReference.labelString, asset =>
        {
            if (asset == null)
            {
                return;
            }

            availablePlaylists.Add(JsonUtility.FromJson<Playlist>(asset.text));
        });
    }

    private async UniTask GetCustomPlaylists()
    {
#if UNITY_ANDROID  && !UNITY_EDITOR
        var path = $"{ANDROIDPATHSTART}{Application.persistentDataPath}{PLAYLISTSFOLDER}";
        #elif UNITY_EDITOR
        var path = UNITYEDITORLOCATION;
#endif
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
                availablePlaylists.Add(playlist);
            }
        }
    }
}