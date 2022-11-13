using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameModeManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

public class SongInfoReader : MonoBehaviour
{
    public static SongInfoReader Instance { get; private set; }

    [TextArea]
    public string info;

    [SerializeField]
    public SongInfo songInfo;

    [SerializeField]
    private DifficultyInfo _difficultyInfo;

    [SerializeField]
    private GameMode _gameMode;

    public UnityEvent<PlaylistItem> finishedLoadingSongInfo = new UnityEvent<PlaylistItem>();

    private CancellationTokenSource _cancellationSource;

    public int Difficulty => _difficultyInfo.DifficultyRank;

    public float NoteSpeed => _difficultyInfo.MovementSpeed;
    public float BeatsPerMinute => songInfo.BeatsPerMinute;

    public GameMode CurrentGameMode => _gameMode;

    #region Const Strings

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string ANDROIDPATHSTART = "file://";
#elif UNITY_EDITOR
    private const string UNITYEDITORLOCATION = "/LocalCustomSongs/Songs/";
    private const string DAT = ".dat";
#endif

    private const string SONGSFOLDER = "/Resources/Songs/";
    private const string LOCALSONGSFOLDER = "Assets/Music/Songs/";
    private const string INFO = "/Info";
    private const string TXT = ".txt";

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
        SubscribeToPlaylistUpdating();
        _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void LoadJson(PlaylistItem item)
    {
        AsyncLoadJson(item).Forget();
    }


    public void CancelLoad()
    {
        _cancellationSource?.Cancel();
    }

    private async UniTaskVoid AsyncLoadJson(PlaylistItem item)
    {
        if (_cancellationSource.IsCancellationRequested)
        {
            _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        }

        try
        {
            if (item.IsCustomSong)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
            var path = $"{Application.persistentDataPath}{SONGSFOLDER}{item.FileLocation}/{INFO}.dat";
#elif UNITY_EDITOR

                var dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
                var path = $"{dataPath}{UNITYEDITORLOCATION}{item.FileLocation}{INFO}{DAT}";
#endif
                if (!File.Exists(path))
                {
                    NotificationManager.ReportFailedToLoadInGame($"{item.SongName} does not exist on your device.");
                    Debug.Log(path + " Doesnt Exist?");
                    return;
                }

                var streamReader = new StreamReader(path);
                var reading = streamReader.ReadToEndAsync().AsUniTask()
                    .AttachExternalCancellation(_cancellationSource.Token);
                var result = await reading;
                streamReader.Close();
                if (string.IsNullOrWhiteSpace(result))
                {
                    LevelManager.Instance.LoadFailed();
                    NotificationManager.ReportFailedToLoadInGame($"{item.SongName}'s info failed to load.");
                    return;
                }
                else
                {
                    UpdateSongInfo(result, item);
                }
            }
            else
            {
                var request =
                    Addressables.LoadAssetAsync<TextAsset>($"{LOCALSONGSFOLDER}{item.FileLocation}{INFO}{TXT}")
                        .ToUniTask().AttachExternalCancellation(_cancellationSource.Token);
                var json = await request;
                if (json == null)
                {
                    LevelManager.Instance.LoadFailed();
                    NotificationManager.ReportFailedToLoadInGame($"{item.SongName}'s info failed to load.");
                    return;
                }

                UpdateSongInfo(json.text, item);
            }

            item.SongInfo = songInfo;
            finishedLoadingSongInfo?.Invoke(item);
        }
        catch (Exception e)when (e is OperationCanceledException)
        {
            if (_cancellationSource.IsCancellationRequested && this?.gameObject != null)
            {
                _cancellationSource =
                    CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            }

            return;
        }
    }

    public void UpdateSongInfo(string json, PlaylistItem item)
    {
        info = json;
        songInfo = JsonUtility.FromJson<SongInfo>(json);
        var playlist = PlaylistManager.Instance.CurrentPlaylist;
        
        if (playlist == null)
        {
            return;
        }
        
        if (playlist.GameModeOverride == GameMode.Unset)
        {
            _gameMode = item.TargetGameMode;
        }
        else
        {
            _gameMode = playlist.GameModeOverride;
        }

        var targetDifficulty = playlist.DifficultyEnum == DifficultyInfo.DifficultyEnum.Unset
            ? item.DifficultyEnum
            : playlist.DifficultyEnum;
        var targetGameMode = playlist.GameModeOverride == GameMode.Unset
            ? item.TargetGameMode
            : playlist.GameModeOverride;

        _difficultyInfo = songInfo.TryGetActiveDifficultyInfo(targetDifficulty, targetGameMode);
    }

    public string GetSongFullName()
    {
        return GetFullSongName(songInfo, _difficultyInfo.Difficulty, _gameMode);
    }

    public AudioClip GetCurrentSong()
    {
        return null;
    }

    private void SubscribeToPlaylistUpdating()
    {
        PlaylistManager.Instance.playlistItemUpdated.AddListener(LoadJson);
    }

    public static string GetFullSongName(SongInfo info, string difficulty, GameMode gameMode)
    {
        return $"{info.SongName}-{difficulty}-{gameMode}-{info.fileLocation}-{info.SongLength}";
    }
}