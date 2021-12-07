using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSongSorting : MonoBehaviour
{
    [SerializeField]
    private SongInfo.SortingMethod _sortingMethod;

    public void SortSongs()
    {
        var method = _sortingMethod;
        if (SongInfoFilesReader.Instance.CurrentSortingMethod == _sortingMethod)
        {
            method++;
        }
        SongInfoFilesReader.Instance.SetSortMethod(method);
    }
}
