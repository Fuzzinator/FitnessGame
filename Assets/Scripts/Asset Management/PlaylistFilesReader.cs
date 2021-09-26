using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class PlaylistFilesReader : MonoBehaviour
{
    public static PlaylistFilesReader Instance { get; private set; }

    public List<Playlist> availablePlaylists = new List<Playlist>();

    [SerializeField]
    private UnityEvent _playlistsUpdated = new UnityEvent();
    
    #region Const Strings

#if UNITY_ANDROID // && !UNITY_EDITOR
    private const string ANDROIDPATHSTART = "file://";
#endif

    private const string PLAYLISTFOLDER = "/Resources/Playlists/";
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

    private void Start()
    {
        UpdateAvailablePlaylists();
    }

    private async UniTaskVoid UpdateAvailablePlaylists()
    {
        availablePlaylists.Clear();
        await GetCustomPlaylists();
        _playlistsUpdated?.Invoke();
        //availablePlaylists = GetCustomPlaylists();
    }

    /*private List<Playlist> GetBuiltInPlaylists()
    {
        
    }*/

    private async UniTask GetCustomPlaylists()
    {
#if UNITY_ANDROID  && !UNITY_EDITOR
        var path = $"{ANDROIDPATHSTART}{Application.persistentDataPath}{PLAYLISTFOLDER}";
        #elif UNITY_EDITOR
        var path = "E:\\Projects\\FitnessGame\\Assets\\Resources\\Playlists";
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