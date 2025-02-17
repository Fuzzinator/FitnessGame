using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SetSongSorting : MonoBehaviour
{
    [SerializeField]
    private SongInfo.SortingMethod _sortingMethod;

    [SerializeField]
    private UnityEvent _songsSorted = new UnityEvent();

    [SerializeField]
    private TMP_Dropdown_XRSupport _dropdown;

    private void OnEnable()
    {
        GetCurrentSort().Forget();
    }

    public void SortSongs(int sortingMethod)
    {
        if (sortingMethod == 0)
        {
            return;
        }
        var method = (SongInfo.SortingMethod)sortingMethod;
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

    private async UniTaskVoid GetCurrentSort()
    {
        await UniTask.NextFrame();
        var currentMethod = SongInfoFilesReader.Instance.CurrentSortingMethod;
        _dropdown.value = (int)currentMethod;
        Sort(SongInfoFilesReader.Instance.CurrentSortingMethod);
    }
}
