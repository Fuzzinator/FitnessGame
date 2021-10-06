using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Android;
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

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string ANDROIDPATHSTART = "file://";
    private const string PLAYLISTSFOLDER = "/Resources/Playlists/";
#elif UNITY_EDITOR
    private const string UNITYEDITORLOCATION = "E:\\Projects\\FitnessGame\\LocalCustomSongs\\Playlists";
#endif

    private const string PLAYLISTEXTENSION = ".txt";

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
        UpdatePlaylists();
    }

    public async void UpdatePlaylists()
    {
        await UpdateAvailablePlaylists();
    }

    private async UniTask UpdateAvailablePlaylists()
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
#if UNITY_ANDROID && !UNITY_EDITOR
        var path = $"{Application.persistentDataPath}{PLAYLISTSFOLDER}";
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
            Debug.Log($"Creating {path}");
        }
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
                streamReader.Close();
            }
        }
    }

    public void AddNewPlaylisy(Playlist playlist)
    {
        availablePlaylists.Add(playlist);
        _playlistsUpdated?.Invoke();
    }
}