using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameModeManagement
{
public class GameModeSetter : MonoBehaviour
{
    [SerializeField]
    private GameMode _targetMode;
    
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
        if (GameManager.Instance == null)
        {
            return;
        }
        GameManager.Instance.SetGameMode(mode);
    }
}

}
