using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSongSorting : MonoBehaviour
{
    [SerializeField]
    private SongInfo.SortingMethod _sortingMethod;

    public void SortSongs(int sortingMethod)
    {
        var method = (SongInfo.SortingMethod) sortingMethod;
        SongInfoFilesReader.Instance.SetSortMethod(method);
    }
    
    public void SortSongs()
    {
        var method = _sortingMethod;
        if (SongInfoFilesReader.Instance.CurrentSortingMethod == method)
        {
            method++;
        }
        SongInfoFilesReader.Instance.SetSortMethod(method);
    }
}
