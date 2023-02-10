using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal sealed class TargetSideInfo
{
    public const string TARGETSIDESETTING = "TargetSideSetting";

    public static void SetSetting(TargetSide setting)
    {
        SettingsManager.SetCachedInt(TARGETSIDESETTING, (int)setting);
    }
    
    public static TargetSide GetSetting()
    {
        var asInt = SettingsManager.GetCachedInt(TARGETSIDESETTING, 0);
        return (TargetSide) asInt;
    }
}

public enum TargetSide
{
    Crossed = 0,
    Uncrossed = 1,
    Mixed = 2
}
