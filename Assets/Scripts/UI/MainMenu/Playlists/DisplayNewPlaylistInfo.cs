using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayNewPlaylistInfo : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _playlistLength;

    public void ShowInfo()
    {
        var length = PlaylistMaker.Instance.GetLength();
        _playlistLength.SetText($"{length:0.00}  minutes");
    }
}
