using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameModeManagement
{
public class GameModeSetter : MonoBehaviour
{
    [SerializeField]
    private GameMode _targetMode;

    [SerializeField]
    private DifficultyInfo.DifficultyEnum _targetDifficulty;
    
    public void SetGameMode(int mode)
    {
        SetGameMode((GameMode)mode);
    }

    public void SetGameModeWithTarget()
    {
        SetGameMode(_targetMode);
    }

    public void SetGameMode(GameMode mode)
    {
        if (PlaylistManager.Instance == null || PlaylistManager.Instance.CurrentPlaylist == null)
        {
            return;
        }
        PlaylistManager.Instance.CurrentPlaylist.SetGameMode(mode);
    }

    public void SetDifficulty(int difficulty)
    {
        if (PlaylistManager.Instance == null || PlaylistManager.Instance.CurrentPlaylist == null)
        {
            return;
        }
        PlaylistManager.Instance.CurrentPlaylist.SetDifficulty(DifficultyInfo.GetDifficultyAsEnum(difficulty));
    }

    public void SetDifficultyWithTarget()
    {
        if (PlaylistManager.Instance == null || PlaylistManager.Instance.CurrentPlaylist == null)
        {
            return;
        }
        PlaylistManager.Instance.CurrentPlaylist.SetDifficulty(_targetDifficulty);
    }
    
}

}
