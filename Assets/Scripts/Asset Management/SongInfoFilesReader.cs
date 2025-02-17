using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UI.Scrollers.Playlists;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using static UnityEngine.XR.Hands.XRHandSubsystemDescriptor;
using Random = UnityEngine.Random;

public class SongInfoFilesReader : MonoBehaviour
{
    public static SongInfoFilesReader Instance { get; private set; }

    [SerializeField]
    private SongInfo.SortingMethod _sortingMethod = SongInfo.SortingMethod.SongName;
    [SerializeField]
    private SongInfo.FilterMethod _filterMethod = SongInfo.FilterMethod.AllSongs;

    public List<SongInfo> allAvailableSongs = new List<SongInfo>();

    public List<SongInfo> filteredAvailableSongs = new List<SongInfo>();

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
    public SongInfo.FilterMethod CurrentFilterMethod => _filterMethod;

    public int CustomSongsCount { get; private set; }

    #region Const Strings

    private const string SONGINFONAME = "Info.txt";
    private const string ALTSONGINFONAME = "Info.dat";
    private const string SortingMethod = "SortingMethod";
    private const string FilterMethod = "FilterMethod";

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
        WaitForProfile().Forget();
    }

    private void Start()
    {
        _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        _destructionCancellationToken = this.GetCancellationTokenOnDestroy();
        UpdateSongs().Forget();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        foreach (var song in allAvailableSongs)
        {
            song.UnloadImage();
        }
    }

    private async UniTaskVoid WaitForProfile()
    {
        await UniTask.WaitWhile(() => ProfileManager.Instance != null && ProfileManager.Instance.ActiveProfile == null);

        if (ProfileManager.Instance == null || ProfileManager.Instance.ActiveProfile == null)
        {
            return;
        }

        _filterMethod = SettingsManager.GetSetting(FilterMethod, SongInfo.FilterMethod.AllSongs);
        _sortingMethod = SettingsManager.GetSetting(SortingMethod, SongInfo.SortingMethod.SongName);
    }

    private async UniTask UpdateSongs()
    {
        _startSongsUpdate?.Invoke();
        await UpdateAvailableSongs();
    }

    private async UniTask UpdateAvailableSongs()
    {
        allAvailableSongs.Clear();
        CustomSongsCount = 0;


        await AssetManager.GetBuiltInSongs(_labelReference, AddSongs);

        await AssetManager.GetCustomSongs(AddCustomSongs, _cancellationSource);

        FilterSongs();
        SortSongs();

        _songsUpdated?.Invoke();
    }

    private void AddSongs(SongInfo info)
    {
        allAvailableSongs.Add(info);
        _songAdded?.Invoke(info);
    }

    private void AddCustomSongs(SongInfo info)
    {
        allAvailableSongs.Add(info);
        _songAdded?.Invoke(info);
        CustomSongsCount++;
    }

    public SongInfo TryGetSongInfo(string folderName)
    {
        return allAvailableSongs.Find((i) => string.Equals(i.fileLocation, folderName));
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

        NotificationManager.RequestNotification(deleteVisuals, () => ConfirmDeleteSong(targetSongInfo));
    }

    public void TryHideSong()
    {
        var targetSongInfo = PlaylistMaker.Instance.DisplayedSongInfo;
        if (targetSongInfo == null || targetSongInfo.isCustomSong)
        {
            return;
        }

        var hideVisuals = new Notification.NotificationVisuals(
            $"Are you sure you would like to hide {targetSongInfo.SongName}?\nIt can be unhidden in the settings menu.",
            "Hide Song?", "Confirm", "Cancel");

        NotificationManager.RequestNotification(hideVisuals, () => ConfirmHideSong(targetSongInfo));
    }

    private void ConfirmDeleteSong(SongInfo targetSongInfo)
    {
        MainMenuUIController.Instance.RequestDisableUI(this);

        allAvailableSongs.Remove(targetSongInfo);
        if (filteredAvailableSongs.Contains(targetSongInfo))
        {
            filteredAvailableSongs.Remove(targetSongInfo);
        }

        CustomSongsCount--;

        _songRemoved?.Invoke(targetSongInfo);
        _songsUpdated?.Invoke();

        AssetManager.DeleteCustomSong(targetSongInfo);

        PlaylistMaker.Instance.SetActiveItem(new SongInfo());
        MainMenuUIController.Instance.RequestEnableUI(this);
    }

    private void ConfirmHideSong(SongInfo targetSongInfo)
    {
        MainMenuUIController.Instance.RequestDisableUI(this);

        AssetManager.HideSong(targetSongInfo.SongID);

        _songRemoved?.Invoke(targetSongInfo);
        _songsUpdated?.Invoke();


        PlaylistMaker.Instance.SetActiveItem(new SongInfo());
        MainMenuUIController.Instance.RequestEnableUI(this);
    }

    public void ResetHiddenSongs()
    {
        _songsUpdated?.Invoke();
    }

    public async UniTask<SongInfo> LoadNewSong(string songFolderName, string songID, float songScore, bool correctBPM)
    {
        var songInfo = await AssetManager.TryGetSingleCustomSong(songFolderName, _cancellationSource.Token, songID, songScore, correctBPM);
        if (songInfo != null)
        {
            if (correctBPM)
            {
                var fileInfo = await BeatSageConverter.ConvertSong(songInfo, _destructionCancellationToken);
                await AssetManager.UpdateMap(songInfo, fileInfo, true, songID, songScore, fileInfo.CreationTime, _cancellationSource.Token);
            }
            var existingSong = allAvailableSongs.Find((info) => (!string.Equals(songID, "LOCAL") && string.Equals(info.SongID, songID, StringComparison.InvariantCultureIgnoreCase)) ||
                                                             string.Equals(info.fileLocation, songInfo.fileLocation, StringComparison.InvariantCultureIgnoreCase));
            if (existingSong != null)
            {
                allAvailableSongs.Remove(existingSong);
                if (filteredAvailableSongs.Contains(existingSong))
                {
                    filteredAvailableSongs.Remove(existingSong);
                }
                CustomSongsCount--;
            }
            allAvailableSongs.Add(songInfo);
            CustomSongsCount++;

            _songAdded?.Invoke(songInfo);
            TryAddToFiltered(songInfo);
            _songsUpdated?.Invoke();
        }
        return songInfo;
    }

    private void TryAddToFiltered(SongInfo song)
    {
        var added = false;
        switch (_filterMethod)
        {
            case SongInfo.FilterMethod.None:
            case SongInfo.FilterMethod.AllSongs:
                filteredAvailableSongs.Add(song);
                break;
            case SongInfo.FilterMethod.BuiltIn:
                if (song.isCustomSong)
                {
                    return;
                }
                filteredAvailableSongs.Add(song);
                break;
            case SongInfo.FilterMethod.AllCustom:
                if (!song.isCustomSong)
                {
                    return;
                }
                filteredAvailableSongs.Add(song);
                break;
            case SongInfo.FilterMethod.Converted:
                {
                    if (!song.isCustomSong)
                    {
                        return;
                    }

                    var autoConverted = string.Equals(song.RecordableName, "LOCAL") || (song.SongName == song.SongID && song.SongScore == 0) || song.AutoConverted;

                    if (!autoConverted)
                    {
                        return;
                    }
                    filteredAvailableSongs.Add(song);
                }
                break;
            case SongInfo.FilterMethod.Downloaded:
                {
                    if (!song.isCustomSong)
                    {
                        return;
                    }

                    var autoConverted = string.Equals(song.RecordableName, "LOCAL") || (song.SongName == song.SongID && song.SongScore == 0) || song.AutoConverted;

                    if (autoConverted)
                    {
                        return;
                    }
                    filteredAvailableSongs.Add(song);
                }
                break;
        }
        SortSongs();
    }

    public void SetSortMethod(SongInfo.SortingMethod method)
    {
        if (_sortingMethod != method)
        {
            _sortingMethod = method;
            SettingsManager.SetSetting(SortingMethod, method);
            SortSongs();
        }
    }

    public void SetFilterMethod(SongInfo.FilterMethod method)
    {
        if (_filterMethod != method)
        {
            _filterMethod = method;
            SettingsManager.SetSetting(FilterMethod, method);
            FilterSongs();
        }
    }

    private void FilterSongs()
    {
        filteredAvailableSongs.Clear();

        switch (_filterMethod)
        {
            case SongInfo.FilterMethod.None:
            case SongInfo.FilterMethod.AllSongs:
                filteredAvailableSongs.AddRange(allAvailableSongs);
                break;
            case SongInfo.FilterMethod.BuiltIn:
                foreach (var song in allAvailableSongs)
                {
                    if (song.isCustomSong)
                    {
                        continue;
                    }
                    filteredAvailableSongs.Add(song);
                }
                break;
            case SongInfo.FilterMethod.AllCustom:
                foreach (var song in allAvailableSongs)
                {
                    if (!song.isCustomSong)
                    {
                        continue;
                    }
                    filteredAvailableSongs.Add(song);
                }
                break;
            case SongInfo.FilterMethod.Converted:
                foreach (var song in allAvailableSongs)
                {
                    if (!song.isCustomSong)
                    {
                        continue;
                    }

                    var autoConverted = string.Equals(song.RecordableName, "LOCAL") || (song.SongName == song.SongID && song.SongScore == 0) || song.AutoConverted;

                    if (!autoConverted)
                    {
                        continue;
                    }
                    filteredAvailableSongs.Add(song);
                }
                break;
            case SongInfo.FilterMethod.Downloaded:
                foreach (var song in allAvailableSongs)
                {
                    if (!song.isCustomSong)
                    {
                        continue;
                    }

                    var autoConverted = string.Equals(song.RecordableName, "LOCAL") || (song.SongName == song.SongID && song.SongScore == 0) || song.AutoConverted;

                    if (autoConverted)
                    {
                        continue;
                    }
                    filteredAvailableSongs.Add(song);
                }
                break;
        }

        SortSongs();
    }

    private void SortSongs()
    {
        switch (_sortingMethod)
        {
            case SongInfo.SortingMethod.None:
                filteredAvailableSongs.Sort((x, y) => Random.Range(-1, 1));
                return;
            case SongInfo.SortingMethod.SongName:
                filteredAvailableSongs.Sort((x, y) =>
                    string.Compare(x.SongName, y.SongName, StringComparison.InvariantCulture));
                break;
            case SongInfo.SortingMethod.InverseSongName:
                filteredAvailableSongs.Sort((x, y) =>
                    string.Compare(y.SongName, x.SongName, StringComparison.InvariantCulture));
                break;
            case SongInfo.SortingMethod.AuthorName:
                filteredAvailableSongs.Sort((x, y) =>
                    string.Compare(x.SongAuthorName, y.SongAuthorName, StringComparison.InvariantCulture));
                break;
            case SongInfo.SortingMethod.InverseAuthorName:
                filteredAvailableSongs.Sort((x, y) =>
                    string.Compare(y.SongAuthorName, x.SongAuthorName, StringComparison.InvariantCulture));
                break;
            case SongInfo.SortingMethod.SongLength:
                filteredAvailableSongs.Sort((x, y) => x.SongLength.CompareTo(y.SongLength));
                break;
            case SongInfo.SortingMethod.InverseSongLength:
                filteredAvailableSongs.Sort((x, y) => y.SongLength.CompareTo(x.SongLength));
                break;
            case SongInfo.SortingMethod.LevelAuthorName:
                filteredAvailableSongs.Sort((x, y) =>
                    string.Compare(x.LevelAuthorName, y.LevelAuthorName, StringComparison.InvariantCulture));
                break;
            case SongInfo.SortingMethod.InverseLevelAuthorName:
                filteredAvailableSongs.Sort((x, y) =>
                    string.Compare(y.LevelAuthorName, x.LevelAuthorName, StringComparison.InvariantCulture));
                break;
            case SongInfo.SortingMethod.BPM:
                filteredAvailableSongs.Sort((x, y) => x.BeatsPerMinute.CompareTo(y.BeatsPerMinute));
                break;
            case SongInfo.SortingMethod.InverseBPM:
                filteredAvailableSongs.Sort((x, y) => y.BeatsPerMinute.CompareTo(x.BeatsPerMinute));
                break;
            case SongInfo.SortingMethod.RecentlyDownloaded:
                filteredAvailableSongs.Sort((x, y) => x.DownloadedDate.CompareTo(y.DownloadedDate));
                break;
            case SongInfo.SortingMethod.InverseRecentlyDownloaded:
                filteredAvailableSongs.Sort((x, y) => y.DownloadedDate.CompareTo(x.DownloadedDate));
                break;
            case SongInfo.SortingMethod.SongScore:
                filteredAvailableSongs.Sort((x, y) => x.SongScore.CompareTo(y.SongScore));
                break;
            case SongInfo.SortingMethod.InverseSongScore:
                filteredAvailableSongs.Sort((x, y) => y.SongScore.CompareTo(x.SongScore));
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