using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class GlobalSettings
{
    public const string USERHEIGHT = "PLAYER_HEIGHT";
    public const string LCONTROLLEROFFSET = "LEFT_CONTROLLER_OFFSET";
    public const string RCONTROLLEROFFSET = "RIGHT_CONTROLLER_OFFSET";

    public static UnityEvent<float> UserHeightChanged = new UnityEvent<float>();
    public static UnityEvent<Vector3> LControllerOffsetChanged = new UnityEvent<Vector3>();
    public static UnityEvent<Vector3> RControllerOffsetChanged = new UnityEvent<Vector3>();
    public static float UserHeight
    {
        get => SettingsManager.GetSetting(USERHEIGHT, -1f);
        set
        {
            SettingsManager.SetSetting(USERHEIGHT, value);
            UserHeightChanged?.Invoke(value);
        }
        
    }

    public static Vector3 LControllerOffset
    {
        get => SettingsManager.GetSetting(LCONTROLLEROFFSET, Vector3.zero);
        set
        {
            SettingsManager.SetSetting(LCONTROLLEROFFSET, value);
            LControllerOffsetChanged?.Invoke(value);
        }
    }

    public static Vector3 RControllerOffset
    {
        get => SettingsManager.GetSetting(RCONTROLLEROFFSET, Vector3.zero);
        set
        {
            SettingsManager.SetSetting(RCONTROLLEROFFSET, value);
            RControllerOffsetChanged?.Invoke(value);
        }
    }
}
