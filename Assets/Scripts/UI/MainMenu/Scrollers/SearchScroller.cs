using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using TMPro;
using UI.Scrollers;
using UnityEngine;

public class SearchScroller : MonoBehaviour
{
    [SerializeField] 
    private TMP_InputField _searchField;

    [SerializeField]
    private ScrollerController _scrollerController;
    
    private void OnEnable()
    {
        ResetSearch();
    }
    
    public void StartEditTextField()
    {
#if UNITY_STANDALONE_WIN
        KeyboardManager.Instance.ActivateKeyboard(_searchField, string.Empty);
#endif
    }

    public void ResetSearch()
    {
        _searchField.ClearText();
    }

    public void FilterSearch(string searchKey)
    {
        _scrollerController.SetSearchKey(searchKey);
    }

    private void OnBecameVisible()
    {
        FilterSearch(string.Empty);
    }
}
