using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class GlobalSettings
{
    public const string USERHEIGHT = "PLAYER_HEIGHT";
    public const string USERHEIGHTOFFSET = "PLAYER_HEIGHT_OFFSET";
    public const string LCONTROLLEROFFSET = "LEFT_CONTROLLER_OFFSET";
    public const string RCONTROLLEROFFSET = "RIGHT_CONTROLLER_OFFSET";

    public static UnityEvent UserHeightChanged = new UnityEvent();
    public static UnityEvent<Vector3> LControllerOffsetChanged = new UnityEvent<Vector3>();
    public static UnityEvent<Vector3> RControllerOffsetChanged = new UnityEvent<Vector3>();
    
    public static float TotalHeight
    {
        get => UserHeight + UserHeightOffset;
    }
    
    public static float UserHeight
    {
        get => SettingsManager.GetSetting(USERHEIGHT, -1f);
        set
        {
            SettingsManager.SetSetting(USERHEIGHT, value);
            UserHeightChanged?.Invoke();
        }

    }
    public static float UserHeightOffset
    {
        get => SettingsManager.GetSetting(USERHEIGHTOFFSET, 0f);
        set
        {
            SettingsManager.SetSetting(USERHEIGHTOFFSET, value);
            UserHeightChanged?.Invoke();
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

    public static float GetUserHeight(Profile overrideProfile = null)
    {
        if (overrideProfile == null)
        {
            return UserHeight;
        }
        else
        {
            return SettingsManager.GetSetting(USERHEIGHT, -1f, true, overrideProfile);
        }
    }
    public static float GetUserHeightOffset(Profile overrideProfile = null)
    {
        if (overrideProfile == null)
        {
            return UserHeightOffset;
        }
        else
        {
            return SettingsManager.GetSetting(USERHEIGHTOFFSET, 0f, true, overrideProfile);
        }
    }

    public static void SetUserHeight(float height, Profile overrideProfile = null)
    {
        if (overrideProfile == null)
        {
            UserHeight = height;
        }
        else
        {
            SettingsManager.SetSetting(USERHEIGHT, height, true, overrideProfile);
        }
    }

    public static void SetUserHeightOffset(float height, Profile overrideProfile = null)
    {
        if (overrideProfile == null)
        {
            UserHeightOffset = height;
        }
        else
        {
            SettingsManager.SetSetting(USERHEIGHTOFFSET, height, true, overrideProfile);
        }
    }
}
