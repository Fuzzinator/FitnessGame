using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class DisplayNewPlaylistInfo : InputFieldController
{
    [SerializeField]
    private TextMeshProUGUI _playlistLength;

    protected override async  UniTask EditTextField()
    {
        await base.EditTextField();
        PlaylistMaker.Instance.SetPlaylistName(_inputField.text);
    }
    
    public void ShowInfo()
    {
        var length = PlaylistMaker.Instance.GetLength();
        _playlistLength.SetText($"{length:0.00}  minutes");
        _inputField.SetTextWithoutNotify(PlaylistMaker.Instance.PlaylistName);
    }
}
