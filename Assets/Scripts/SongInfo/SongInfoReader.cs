using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Text;
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

    public UnityEvent<PlaylistItem> finishedLoadingSongInfo = new UnityEvent<PlaylistItem>();

    private CancellationTokenSource _cancellationSource;

    public int Difficulty => _difficultyInfo.DifficultyRank;
    public float NoteSpeed => _difficultyInfo.MovementSpeed;
    public float BeatsPerMinute => songInfo.BeatsPerMinute;

    public DifficultyInfo.DifficultyEnum CurrentDifficulty => _difficultyInfo.DifficultyAsEnum;

    #region Const Strings

    private const string INFO = "/Info";
    private const string ConvertedInfo = "/ConvertedInfo";
    private const string TXT = ".txt";
    private const string DAT = ".dat";
    private const string DASH = "-";

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
            _cancellationSource.Dispose();
            _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        }

        try
        {
            if (item.IsCustomSong)
            {
                var infoName = item.UseConvertedFileNames ? ConvertedInfo : INFO;
                var path = $"{AssetManager.SongsPath}{item.FileLocation}{infoName}{DAT}";
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
                var request = Addressables.LoadAssetAsync<TextAsset>($"{AssetManager.LOCALSONGSFOLDER}{item.FileLocation}{INFO}{TXT}");
                var json = await request.ToUniTask().AttachExternalCancellation(_cancellationSource.Token);
                if (json == null)
                {
                    Addressables.Release(request);
                    LevelManager.Instance.LoadFailed();
                    NotificationManager.ReportFailedToLoadInGame($"{item.SongName}'s info failed to load.");
                    return;
                }

                UpdateSongInfo(json.text, item);
                Addressables.Release(request);
            }

            item.SongInfo = songInfo;
            finishedLoadingSongInfo?.Invoke(item);
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
            if (_cancellationSource.IsCancellationRequested && this?.gameObject != null)
            {
                _cancellationSource.Dispose();
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

        var targetDifficulty = PlaylistManager.Instance.TargetDifficulty;
        var targetGameMode = PlaylistManager.Instance.TargetGameMode;

        _difficultyInfo = songInfo.TryGetActiveDifficultyInfo(targetDifficulty, targetGameMode);
    }

    public string GetSongFullName()
    {
        var gameMode = PlaylistManager.Instance.TargetGameMode;
        return GetFullSongNameNoID(songInfo, _difficultyInfo.DifficultyAsEnum, gameMode, null, null);
    }

    public AudioClip GetCurrentSong()
    {
        return null;
    }

    private void SubscribeToPlaylistUpdating()
    {
        PlaylistManager.Instance.playlistItemUpdated.AddListener(LoadJson);
    }

    public static string GetFullSongName(SongInfo info, DifficultyInfo.DifficultyEnum difficultyEnum, GameMode gameMode, string prefix = null, string suffix = null)
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                sb.Append(prefix);
            }

            sb.Append(info.SongName);
            sb.Append(DASH);
            sb.Append(difficultyEnum.Readable());
            sb.Append(DASH);
            sb.Append(gameMode);
            sb.Append(DASH);
            sb.Append(info.SongID);

            if (!string.IsNullOrWhiteSpace(suffix))
            {
                sb.Append(suffix);
            }

            return sb.ToString();
        }
    }

    public static string GetFullSongNameNoID(SongInfo info, DifficultyInfo.DifficultyEnum difficultyEnum, GameMode gameMode, string prefix = null, string suffix = null)
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                sb.Append(prefix);
            }

            sb.Append(info.SongName);
            sb.Append(DASH);
            sb.Append(difficultyEnum.Readable());
            sb.Append(DASH);
            sb.Append(gameMode);
            sb.Append(DASH);
            sb.Append(info.fileLocation);
            sb.Append(DASH);
            sb.Append(info.SongLength);

            if (!string.IsNullOrWhiteSpace(suffix))
            {
                sb.Append(suffix);
            }

            return sb.ToString();
        }
    }
}