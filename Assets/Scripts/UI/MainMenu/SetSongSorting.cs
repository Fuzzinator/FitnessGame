using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SetSongSorting : MonoBehaviour
{
    [SerializeField]
    private SongInfo.SortingMethod _sortingMethod;

    [SerializeField]
    private UnityEvent _songsSorted = new UnityEvent();

    public void SortSongs(int sortingMethod)
    {
        var method = (SongInfo.SortingMethod) (sortingMethod+1);
        Sort(method);
    }
    
    public void SortSongs()
    {
        var method = _sortingMethod;
        if (SongInfoFilesReader.Instance.CurrentSortingMethod == method)
        {
            method++;
        }
        Sort(method);
    }

    private void Sort(SongInfo.SortingMethod method)
    {
        SongInfoFilesReader.Instance.SetSortMethod(method);
        _songsSorted?.Invoke();
    }
}
