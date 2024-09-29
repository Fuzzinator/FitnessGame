using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

public class PlaylistFilesReader : MonoBehaviour
{
    public static PlaylistFilesReader Instance { get; private set; }

    [SerializeField]
    private Playlist.SortingMethod _sortingMethod = Playlist.SortingMethod.PlaylistName;

    public List<Playlist> availablePlaylists = new List<Playlist>();

    [SerializeField]
    private AssetLabelReference _labelReference;

    [SerializeField]
    private UnityEvent _playlistsUpdated = new UnityEvent();

    public Playlist.SortingMethod CurrentSortingMethod => _sortingMethod;

    private CancellationToken _cancellationToken;

    private List<AsyncOperationHandle> _assetHandles = new List<AsyncOperationHandle>();

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
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    private void OnDestroy()
    {
        foreach (var assetHandle in _assetHandles)
        {
            Addressables.Release(assetHandle);
        }
        _assetHandles.Clear();
    }

    public void RequestPlaylistsUpdate()
    {
        UpdatePlaylists().Forget();
    }

    public async UniTaskVoid UpdatePlaylists()
    {
        await UpdateAvailablePlaylists();
    }

    public async UniTaskVoid RefreshPlaylistsValidStates()
    {
        for (int i = 0; i < availablePlaylists.Count; i++)
        {
            var playlist = availablePlaylists[i];
            if (playlist == null)
            {
                continue;
            }
            playlist.isValid = await PlaylistValidator.IsValid(playlist);
        }
    }

    private async UniTask UpdateAvailablePlaylists()
    {
        availablePlaylists.Clear();
        void AddPlaylist(Playlist playlist)
        {
            if (playlist == null)
            {
                return;
            }

            availablePlaylists.Add(playlist);
            _playlistsUpdated?.Invoke();
        }

        var handles = await AssetManager.GetBuiltInPlaylists(_labelReference.labelString, AddPlaylist, _cancellationToken);
        await AssetManager.GetCustomPlaylists(AddPlaylist, _cancellationToken);

        if (handles != null)
        {
            _assetHandles.AddRange(handles);
        }

        SortPlaylists();
        _playlistsUpdated?.Invoke();
    }

    public void AddNewPlaylist(Playlist playlist)
    {
        if (playlist == null)
        {
            return;
        }

        //var existingIndex = availablePlaylists.IndexOf(playlist);//Find((i) => i.PlaylistName == playlist.PlaylistName);
        if (!availablePlaylists.Contains(playlist))
        {
            availablePlaylists.Add(playlist);
            SortPlaylists();
        }
        _playlistsUpdated?.Invoke();
    }

    public void RemovePlaylist(Playlist playlist)
    {
        if (playlist == null)
        {
            return;
        }
        var index = availablePlaylists.FindIndex((i) => i.GUID.Equals(playlist.GUID));
        availablePlaylists.RemoveAt(index);
        _playlistsUpdated?.Invoke();
    }

    public void SetSortMethod(Playlist.SortingMethod method)
    {
        if (_sortingMethod != method)
        {
            _sortingMethod = method;
            SortPlaylists();
        }
    }

    private void SortPlaylists()
    {
        switch (_sortingMethod)
        {
            case Playlist.SortingMethod.None:
                availablePlaylists.Sort((x, y) => Random.Range(-1, 1));
                return;
            case Playlist.SortingMethod.PlaylistName:
                availablePlaylists.Sort((x, y) =>
                    string.Compare(x.PlaylistName, y.PlaylistName, StringComparison.InvariantCulture));
                break;
            case Playlist.SortingMethod.InversePlaylistName:
                availablePlaylists.Sort((x, y) =>
                    string.Compare(y.PlaylistName, x.PlaylistName, StringComparison.InvariantCulture));
                break;
            case Playlist.SortingMethod.PlaylistLength:
                availablePlaylists.Sort((x, y) => x.Length.CompareTo(y.Length));
                break;
            case Playlist.SortingMethod.InversePlaylistLength:
                availablePlaylists.Sort((x, y) => y.Length.CompareTo(x.Length));
                break;
        }
    }

    public void RemovePlaylistByName(string playlistName)
    {
        for (var i = 0; i < availablePlaylists.Count; i++)
        {
            if (availablePlaylists[i].PlaylistName == playlistName)
            {
                availablePlaylists.RemoveAt(i);
                _playlistsUpdated?.Invoke();
                break;
            }
        }
    }

    public void TryHidePlaylist()
    {
        var currentPlaylist = PlaylistManager.Instance.CurrentPlaylist;
        if (currentPlaylist == null)
        {
            return;
        }

        var playlist = currentPlaylist.PlaylistName;
        var hideVisuals = new Notification.NotificationVisuals(
            $"Are you sure you would like to hide {playlist}?\nIt can be unhidden in the settings menu.",
            "Hide Workout?", "Confirm", "Cancel");

        NotificationManager.RequestNotification(hideVisuals, () => HidePlaylist(currentPlaylist));
    }

    private void HidePlaylist(Playlist currentPlaylist)
    {
        AssetManager.HidePlaylist(currentPlaylist.GUID);
        Instance.RemovePlaylist(currentPlaylist);

        PlaylistManager.Instance.CurrentPlaylist = null;
    }

    public void ResetHiddenPlaylists()
    {
        _playlistsUpdated?.Invoke();
    }
}