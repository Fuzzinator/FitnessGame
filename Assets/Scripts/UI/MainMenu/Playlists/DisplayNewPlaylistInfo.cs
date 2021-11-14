using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class DisplayNewPlaylistInfo : MonoBehaviour
{
    [FormerlySerializedAs("_textField")] [SerializeField]
    private TMP_InputField _playlistName;

    [SerializeField]
    private string _defaultText;
    
    [SerializeField]
    private TextMeshProUGUI _playlistLength;

    public async void StartEditTextField()
    {
        var keyboard = KeyboardManager.Instance.ActivateKeyboard(_playlistName, _defaultText);
        await UniTask.WaitWhile(() => keyboard.gameObject.activeInHierarchy);
        PlaylistMaker.Instance.SetPlaylistName(_playlistName.text);

    }
    
    public void ShowInfo()
    {
        var length = PlaylistMaker.Instance.GetLength();
        _playlistLength.SetText($"{length:0.00}  minutes");
        _playlistName.SetTextWithoutNotify(PlaylistMaker.Instance.PlaylistName);
    }
}
