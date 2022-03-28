using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
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
        _playlistLength.SetText(PlaylistMaker.Instance.GetReadableLength());
        _inputField.SetTextWithoutNotify(PlaylistMaker.Instance.PlaylistName);
    }
}
