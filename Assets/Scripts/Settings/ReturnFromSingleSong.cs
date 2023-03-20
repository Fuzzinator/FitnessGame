using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using UI;
using UI.Scrollers.Playlists;
using UnityEngine;

public class ReturnFromSingleSong : MonoBehaviour
{
    [SerializeField]
    private AvailableSongInfoScrollerController _availableSongInfoScrollerController;
    [SerializeField]
    private DisplaySongRecords _availableSongsRecordDisplay;
    [SerializeField]
    private DisplaySongInfo _availableSongsInfoDisplay;
    void Start()
    {
        var currentPlaylist = PlaylistManager.Instance.CurrentPlaylist;
        if(currentPlaylist == null || !string.IsNullOrWhiteSpace(currentPlaylist.GUID) || currentPlaylist.Items.Length != 1)
        {
            return;
        }

        MainMenuUIController.Instance.SetActivePage(3);
        DelayAndUpdateDisplay(currentPlaylist).Forget();
    }

    private async UniTaskVoid DelayAndUpdateDisplay(Playlist currentPlaylist)
    {
        await UniTask.DelayFrame(1);
        var songInfo = currentPlaylist.Items[0].SongInfo;
        _availableSongInfoScrollerController.ScrollToData(songInfo);
        _availableSongsRecordDisplay.ShowRecords(songInfo);
        _availableSongsInfoDisplay.RequestDisplay(songInfo);
        PlaylistMaker.Instance.SetActiveItem(songInfo);
    }
}
