using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UI.Scrollers.Playlists;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

    [SerializeField]
    private UnityEvent<SongInfo> _songAdded = new UnityEvent<SongInfo>();

    [SerializeField]
    private UnityEvent<SongInfo> _songRemoved = new UnityEvent<SongInfo>();

    public UnityEvent<SongInfo> SongAdded => _songAdded;

    [Header("UI Thingy")]
    [SerializeField]
    private DisplaySongInfo _displaySongInfo;

    private CancellationTokenSource _cancellationSource;
    private CancellationToken _destructionCancellationToken;

    public SongInfo.SortingMethod CurrentSortingMethod => _sortingMethod;

    #region Const Strings

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

        foreach(var song in availableSongs)
        {
            song.UnloadImage();
        }
    }

    private void OnEnable()
    {
        UpdateSongs().Forget();
    }

    public async UniTask UpdateSongs()
    {
        _startSongsUpdate?.Invoke();
        await UpdateAvailableSongs();
    }

    private async UniTask UpdateAvailableSongs()
    {
        availableSongs.Clear();

        void AddSongs(SongInfo info)
        {
            availableSongs.Add(info);
            _songAdded?.Invoke(info);
        }

        await AssetManager.GetBuiltInSongs(_labelReference, AddSongs);
        await AssetManager.GetCustomSongs(AddSongs, _cancellationSource);
        SortSongs();
        _songsUpdated?.Invoke();
    }

    public void TryDeleteSong()
    {
        var targetSongInfo = PlaylistMaker.Instance.DisplayedSongInfo;
        if (!targetSongInfo.isCustomSong)
        {
            return;
        }

        var deleteVisuals = new Notification.NotificationVisuals(
            $"Are you sure you would like to permanently delete {targetSongInfo.SongName}?",
            "Delete Song?", "Confirm", "Cancel");

        NotificationManager.RequestNotification(deleteVisuals, () => ConfirmDeleteSong(targetSongInfo).Forget());
    }

    private async UniTaskVoid ConfirmDeleteSong(SongInfo targetSongInfo)
    {
        MainMenuUIController.Instance.RequestDisableUI(this);

        availableSongs.Remove(targetSongInfo);
        _songRemoved?.Invoke(targetSongInfo);
        _songsUpdated?.Invoke();

        await AssetManager.DeleteCustomSong(targetSongInfo);
        //_displaySongInfo.ClearDisplayedInfo();
        PlaylistMaker.Instance.SetActiveItem(new SongInfo());
        MainMenuUIController.Instance.RequestEnableUI(this);
    }

    public async UniTask LoadNewSong(string songFolderName)
    {
        var songInfo = await AssetManager.TryGetSingleCustomSong(songFolderName, _cancellationSource.Token);
        if (songInfo != null)
        {
            availableSongs.Add(songInfo);
            _songAdded?.Invoke(songInfo);
            SortSongs();
            _songsUpdated?.Invoke();
        }
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
                    string.Compare(x.SongName, y.SongName, StringComparison.InvariantCulture));
                break;
            case SongInfo.SortingMethod.InverseSongName:
                availableSongs.Sort((x, y) =>
                    string.Compare(y.SongName, x.SongName, StringComparison.InvariantCulture));
                break;
            case SongInfo.SortingMethod.AuthorName:
                availableSongs.Sort((x, y) =>
                    string.Compare(x.SongAuthorName, y.SongAuthorName, StringComparison.InvariantCulture));
                break;
            case SongInfo.SortingMethod.InverseAuthorName:
                availableSongs.Sort((x, y) =>
                    string.Compare(y.SongAuthorName, x.SongAuthorName, StringComparison.InvariantCulture));
                break;
            case SongInfo.SortingMethod.SongLength:
                availableSongs.Sort((x, y) => x.SongLength.CompareTo(y.SongLength));
                break;
            case SongInfo.SortingMethod.InverseSongLength:
                availableSongs.Sort((x, y) => y.SongLength.CompareTo(x.SongLength));
                break;
            case SongInfo.SortingMethod.LevelAuthorName:
                availableSongs.Sort((x, y) =>
                    string.Compare(x.LevelAuthorName, y.LevelAuthorName, StringComparison.InvariantCulture));
                break;
            case SongInfo.SortingMethod.InverseLevelAuthorName:
                availableSongs.Sort((x, y) =>
                    string.Compare(y.LevelAuthorName, x.LevelAuthorName, StringComparison.InvariantCulture));
                break;
        }
    }
    /*private struct CreateMissingDifficultiesJob : IJobParallelFor
    {
        private NativeArray<SongInfo.DifficultySet> _difficultySets;
        public NativeArray<SongInfo.DifficultySet> DifficultySets => _difficultySets;
        public CreateMissingDifficultiesJob(NativeArray<SongInfo.DifficultySet> difficultySets)
        {
            _difficultySets = difficultySets;
        }
        
        public void Execute(int index)
        {
            
        }
    }*/
}