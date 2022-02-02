using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class SongInfoFilesReader : MonoBehaviour
{
    public static SongInfoFilesReader Instance { get; private set; }

    [SerializeField]
    private SongInfo.SortingMethod _sortingMethod = SongInfo.SortingMethod.SongName;

    public List<SongInfo> availableSongs = new List<SongInfo>();

    [SerializeField]
    private AssetLabelReference _labelReference;

    [SerializeField]
    private UnityEvent _startSongsUpdate = new UnityEvent();

    [SerializeField]
    private UnityEvent _songsUpdated = new UnityEvent();

    private CancellationTokenSource _cancellationSource;
    private CancellationToken _destructionCancellationToken;

    public SongInfo.SortingMethod CurrentSortingMethod => _sortingMethod;

    #region Const Strings

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string ANDROIDPATHSTART = "file://";
    private const string SONGSFOLDER = "/Resources/Songs/";
#elif UNITY_EDITOR
    private const string UNITYEDITORLOCATION =  "/LocalCustomSongs/Songs/";
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

    private void Start()
    {
        _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        _destructionCancellationToken = this.GetCancellationTokenOnDestroy();
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnEnable()
    {
        UpdateSongs().Forget();
    }

    public async UniTaskVoid UpdateSongs()
    {
        _startSongsUpdate?.Invoke();
        await UpdateAvailableSongs();
    }

    private async UniTask UpdateAvailableSongs()
    {
        availableSongs.Clear();
        await GetBuiltInSongs();
        await GetCustomSongs();
        SortSongs();
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
#elif UNITY_EDITOR
        var dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
        var path = $"{dataPath}{UNITYEDITORLOCATION}";
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

        foreach (var dir in directories)
        {
            var info = new DirectoryInfo(dir);
            var files = info.GetFiles();
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
                        .AttachExternalCancellation(_destructionCancellationToken);
                    
                    var item = JsonUtility.FromJson<SongInfo>(result);

                    streamReader.Close();

                    for (var i = 0; i < item.DifficultySets.Length; i++)
                    {
                        item.DifficultySets[i].TryCreateMissingDifficulties();
                        item.DifficultySets[i].RemoveExpertPlus();
                    }
                    
                    if (file.Directory != null)
                    {
                        item.fileLocation = file.Directory.Name;
                    }

                    item.isCustomSong = true;
                    if (item.SongLength < 1)
                    {
                        var songLength = await TryGetSongLength(item, songLoader);
                        item.SongLength = songLength;
                        using (var streamWriter = new StreamWriter(file.FullName))
                        {
                            await streamWriter.WriteAsync(JsonUtility.ToJson(item));
                        }
                    }

                    availableSongs.Add(item);
                }
            }
        }
    }

    public async UniTask<float> TryGetSongLength(SongInfo info, SongLoader songLoader,
        bool customSong = true)
    {
        if (songLoader != null)
        {
            UniTask<AudioClip> clipRequest;
            if (customSong)
            {
                clipRequest = songLoader.LoadCustomSong(info.fileLocation, info, _cancellationSource.Token);
            }
            else
            {
                clipRequest = songLoader.LoadBuiltInSong(info, _cancellationSource.Token);
            }

            var audioClip = await clipRequest;
            if (audioClip == null)
            {
                if (_cancellationSource.IsCancellationRequested)
                {
                    _cancellationSource =
                        CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
                }
                else
                {
                    Debug.LogError($"Failed to load {info.SongName}");
                }

                return 0;
            }

            return audioClip.length;
        }

        return 0;
    }

    public void SetSortMethod(SongInfo.SortingMethod method)
    {
        if (_sortingMethod != method)
        {
            _sortingMethod = method;
            SortSongs();
        }
    }

    private void SortSongs()
    {
        switch (_sortingMethod)
        {
            case SongInfo.SortingMethod.None:
                availableSongs.Sort((x, y) => Random.Range(-1, 1));
                return;
            case SongInfo.SortingMethod.SongName:
                availableSongs.Sort((x, y) =>
                    string.Compare(x.SongName, y.SongName, StringComparison.Ordinal));
                break;
            case SongInfo.SortingMethod.InverseSongName:
                availableSongs.Sort((x, y) =>
                    string.Compare(y.SongName, x.SongName, StringComparison.Ordinal));
                break;
            case SongInfo.SortingMethod.AuthorName:
                availableSongs.Sort((x, y) =>
                    string.Compare(x.SongAuthorName, y.SongAuthorName, StringComparison.Ordinal));
                break;
            case SongInfo.SortingMethod.InverseAuthorName:
                availableSongs.Sort((x, y) =>
                    string.Compare(y.SongAuthorName, x.SongAuthorName, StringComparison.Ordinal));
                break;
            case SongInfo.SortingMethod.SongLength:
                availableSongs.Sort((x, y) => x.SongLength.CompareTo(y.SongLength));
                break;
            case SongInfo.SortingMethod.InverseSongLength:
                availableSongs.Sort((x, y) => y.SongLength.CompareTo(x.SongLength));
                break;
        }
    }
}