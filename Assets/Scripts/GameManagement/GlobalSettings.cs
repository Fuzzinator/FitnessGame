using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class GlobalSettings
{
    public const string GAMENAME = "FitnessGame";
    public const string USERHEIGHT = "PLAYER_HEIGHT";

    public static UnityEvent<float> UserHeightChanged = new UnityEvent<float>();
    public static float UserHeight
    {
        get => PlayerPrefs.GetFloat($"{GAMENAME}_{USERHEIGHT}");
        set
        {
            PlayerPrefs.SetFloat($"{GAMENAME}_{USERHEIGHT}", value);
            UserHeightChanged?.Invoke(value);
        }
        
    }
}
