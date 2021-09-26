using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;

public class PlaylistCellView : EnhancedScrollerCellView
{
    [SerializeField]
    private TextMeshProUGUI _playlistName;

    private Playlist _playlist;
    public void SetData(Playlist playlist)
    {
        _playlist = playlist;
        _playlistName.SetText(playlist.PlaylistName);
    }

    public void SetActivePlaylist()
    {
        PlaylistManager.Instance.SetActivePlaylist(_playlist);
    }
}
