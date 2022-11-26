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
        KeyboardManager.Instance.ActivateKeyboard(_searchField, string.Empty);
    }

    public void ResetSearch()
    {
        _searchField.text = string.Empty;
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
