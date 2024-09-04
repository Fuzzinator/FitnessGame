using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using static UnityEngine.Rendering.DebugUI;

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
        _playlistLength.SetTextZeroAlloc(PlaylistMaker.Instance.GetReadableLength(), true);
        _inputField.SetTextWithoutNotify(PlaylistMaker.Instance.PlaylistName);
    }

    public void UpdatePlaylistLength(float speedMod)
    {
        var moddedSpeedMod = SongSliderToPlaylistSpeedMod(speedMod);

        _playlistLength.SetTextZeroAlloc(PlaylistMaker.Instance.GetReadableLength(moddedSpeedMod), true); 
    }

    private float SongSliderToPlaylistSpeedMod(float sliderValue)
    {
        switch ((int)sliderValue)
        {
            case 0:
                return .75f;
            case 1:
                return .875f;
            case 2:
                return 1;
            case 3:
                return 1.125f;
            case 4:
                return 1.25f;
            default:
                return 1;
        }
    }
}
