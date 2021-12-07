using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuffleSongs : MonoBehaviour
{
    public void ShuffleCreatingPlaylist()
    {
        if (PlaylistMaker.Instance == null)
        {
            return;
        }
        PlaylistMaker.Instance.ShufflePlaylistItems();
    }

    public void ShuffleActivePlaylist()
    {
        if (PlaylistManager.Instance == null)
        {
            return;
        }
        
        PlaylistManager.Instance.CurrentPlaylist.ShuffleItems();
    }
}
