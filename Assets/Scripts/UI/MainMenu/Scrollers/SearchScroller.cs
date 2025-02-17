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
    private string _searchName;
    [SerializeField] 
    private TMP_InputField _searchField;

    [SerializeField]
    private ScrollerController _scrollerController;

#if UNITY_ANDROID
    private TouchScreenKeyboard _keyboard;
#endif

    private void OnEnable()
    {
        _searchField.text = SettingsManager.GetSearchFilter(_searchName);
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
        SaveSearch(null);
    }

    public void FilterSearch(string searchKey)
    {
        _scrollerController.SetSearchKey(searchKey);
    }

    private void OnBecameVisible()
    {
        FilterSearch(string.Empty);
    }

    public void SaveSearch(string searchKey)
    {
        if(string.IsNullOrWhiteSpace(_searchName))
        {
            return;
        }

        SettingsManager.SaveSearchFilter(_searchName, searchKey);
    }
}
