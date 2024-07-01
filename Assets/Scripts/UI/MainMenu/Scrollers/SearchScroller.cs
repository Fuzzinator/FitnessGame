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

#if UNITY_ANDROID
    private TouchScreenKeyboard _keyboard;
#endif

    private void OnEnable()
    {
        ResetSearch();
    }
    
    public void StartEditTextField()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        KeyboardManager.Instance.ActivateKeyboard(_searchField, string.Empty);
#elif UNITY_ANDROID
        _keyboard = TouchScreenKeyboard.Open(_searchField.text, TouchScreenKeyboardType.Search);
        FocusTracker.Instance.OnFocusChanged.AddListener(EditFieldCompleteFromFocus);
#endif
    }

#if UNITY_ANDROID
    private void EditFieldCompleteFromFocus(bool focusState)
    {
        if (!focusState)
        {
            return;
        }

        _searchField.text = _keyboard.text;
    }
#endif

    public void ResetSearch()
    {
        _searchField.text = null;
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
