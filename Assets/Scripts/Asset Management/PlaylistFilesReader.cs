using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.ResourceLocations;
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
            playlist.isValid = await PlaylistValidator.IsValid(playlist);
            availablePlaylists[i] = playlist;
        }
    }

    private async UniTask UpdateAvailablePlaylists()
    {
        availablePlaylists.Clear();
        void AddPlaylist(Playlist playlist) => availablePlaylists.Add(playlist);
        await AssetManager.GetBuiltInPlaylists(_labelReference.labelString, AddPlaylist);
        await AssetManager.GetCustomPlaylists(AddPlaylist);
        
        SortPlaylists();
        _playlistsUpdated?.Invoke();
        //availablePlaylists = GetCustomPlaylists();
    }
    
    public void AddNewPlaylist(Playlist playlist)
    {
        var existingPlaylist = availablePlaylists.Find((i) => i.PlaylistName == playlist.PlaylistName);
        if (!string.IsNullOrWhiteSpace(existingPlaylist.PlaylistName))
        {
            availablePlaylists[availablePlaylists.IndexOf(existingPlaylist)] = playlist;
        }
        else
        {
            availablePlaylists.Add(playlist);
        }
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
                availablePlaylists.Sort((x,y) => Random.Range(-1,1));
                return;
            case Playlist.SortingMethod.PlaylistName:
                availablePlaylists.Sort((x, y) => 
                    string.Compare(x.PlaylistName, y.PlaylistName, StringComparison.Ordinal));
                break;
            case Playlist.SortingMethod.InversePlaylistName:
                availablePlaylists.Sort((x, y) => 
                    string.Compare(y.PlaylistName, x.PlaylistName, StringComparison.Ordinal));
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
                break;
            }
        }
    }
}