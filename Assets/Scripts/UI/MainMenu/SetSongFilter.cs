using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SetSongFilter : MonoBehaviour
{
    [SerializeField]
    private SongInfo.FilterMethod _filterMethod;

    [SerializeField]
    private UnityEvent _songsFiltered = new UnityEvent();

    [SerializeField]
    private TMP_Dropdown_XRSupport _dropdown;

    private void OnEnable()
    {
        GetCurrentFilter().Forget();
    }

    public void FilterSongs(int sortingMethod)
    {
        if (sortingMethod == 0)
        {
            return;
        }
        var method = (SongInfo.FilterMethod)sortingMethod;
        Filter(method);
    }

    public void FilterSongs()
    {
        var method = _filterMethod;
        Filter(method);
    }

    private void Filter(SongInfo.FilterMethod method)
    {
        SongInfoFilesReader.Instance.SetFilterMethod(method);
        _songsFiltered?.Invoke();
    }

    private async UniTaskVoid GetCurrentFilter()
    {
        await UniTask.NextFrame();
        var currentMethod = (int)SongInfoFilesReader.Instance.CurrentSortingMethod;
        if(_dropdown.value == currentMethod)
        {
            return;
        }
        _dropdown.value = currentMethod;
    }
}
