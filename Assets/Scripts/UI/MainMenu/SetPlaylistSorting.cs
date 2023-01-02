using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SetPlaylistSorting : MonoBehaviour
{
    [SerializeField]
    private Playlist.SortingMethod _sortingMethod;
    
    [SerializeField]
    private UnityEvent _songsSorted = new UnityEvent();
    public void SortSongs(int sortingMethod)
    {
        var method = (Playlist.SortingMethod) (sortingMethod+1);
        Sort(method);
    }
    
    public void SortPlaylists()
    {
        var method = _sortingMethod;
        if (PlaylistFilesReader.Instance.CurrentSortingMethod == _sortingMethod)
        {
            method++;
        }
        Sort(method);
    }
    
    private void Sort(Playlist.SortingMethod method)
    {
        PlaylistFilesReader.Instance.SetSortMethod(method);
        _songsSorted?.Invoke();
    }
}
