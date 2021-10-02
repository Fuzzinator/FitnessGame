using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongInfoCellView : EnhancedScrollerCellView
{
    [SerializeField]
    private TextMeshProUGUI _songName;
    [SerializeField]
    private TextMeshProUGUI _songAuthor;

    [SerializeField]
    private Button _button;

    private SongInfo _songInfo;
    private EnhancedScroller _scroller;

    private void OnValidate()
    {
        if (_button == null)
        {
            TryGetComponent(out _button);
        }
    }

    public void SetData(SongInfo info, EnhancedScroller scroller)
    {
        _songName.SetText(info.SongName);
        _songAuthor.SetText(info.SongAuthorName);
        _songInfo = info;
        _scroller = scroller;
    }

    public void SetActiveSongInfo()
    {
        var controller = _scroller.Delegate as SongInfoScrollerController;
        controller.SetActiveInfo(_songInfo);
    }
}
