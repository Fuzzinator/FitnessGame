using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using UI;
using UI.Scrollers.Playlists;
using UnityEngine;

public class ReturnToMenu : MonoBehaviour
{
    [SerializeField]
    private AvailableSongInfoScrollerController _availableSongInfoScrollerController;
    [SerializeField]
    private DisplaySongRecords _availableSongsRecordDisplay;
    [SerializeField]
    private DisplaySongInfo _availableSongsInfoDisplay;
    [SerializeField]
    private SetAndShowSongOptions _songOptions;

    void Start()
    {
        var currentPlaylist = PlaylistManager.Instance.CurrentPlaylist;
        var targetActivePage = MainMenuStateTracker.Instance.ActivePage;

        if (targetActivePage <= 0)
        {
            return;
        }

        MainMenuUIController.Instance.SetActivePage(targetActivePage);
        switch (targetActivePage)
        {
            case 1://View Playlists
                PlaylistManager.Instance.CurrentPlaylist = currentPlaylist;
                break;
            case 3:// 3 is Single Song
                SetSingleSongPlaylist(currentPlaylist).Forget();
                break;
            case 7://View Playlist
                PlaylistManager.Instance.CurrentPlaylist = currentPlaylist;
                break;
            default:
                break;
        }
    }

    private async UniTaskVoid SetSingleSongPlaylist(Playlist currentPlaylist)
    {
        await UniTask.DelayFrame(1);
        var playlistItem = currentPlaylist.Items[0];
        _availableSongInfoScrollerController.ScrollToData(playlistItem.SongInfo);
        _availableSongsRecordDisplay.SetSongInfo(playlistItem.SongInfo);
        _availableSongsInfoDisplay.RequestDisplay(playlistItem.SongInfo);
        PlaylistMaker.Instance.SetActiveItem(playlistItem.SongInfo);
        _songOptions.SetSelectedDifAndMode(playlistItem.DifficultyEnum, playlistItem.TargetGameMode);
    }
}
