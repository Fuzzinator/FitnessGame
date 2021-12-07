using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPlaylistSorting : MonoBehaviour
{
    [SerializeField]
    private Playlist.SortingMethod _sortingMethod;

    public void SortPlaylists()
    {
        var method = _sortingMethod;
        if (PlaylistFilesReader.Instance.CurrentSortingMethod == _sortingMethod)
        {
            method++;
        }
        PlaylistFilesReader.Instance.SetSortMethod(method);
    }
}
