using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UpdateToProfiles : MonoBehaviour
{
    private const string PLAYEDBEFORE = "HasPlayerPlayedBefore";
    private const string USEMETERS = "DisplayInMeters";
    private const string CUSTOMCOLORSETCOUNT = "CustomColorSetCount";
    private const string CUSTOMCOLORSETNUMBERX = "CustomColorSetNumber:";
    private const string ACTIVECOLORINDEX = "ActiveColorSetIndex";
    private const string CUSTOMCOLORS = "CustomColors";
    private const string LEFTHANDED = "LeftHanded";
    private const string RIGHTGLOVEROTATION = "RightGloveRotationOffset";
    private const string LEFTGLOVEROTATION = "LeftGloveRotationOffset";
    private const string RIGHTGLOVEOFFSET = "RightGloveOffset";
    private const string LEFTGLOVEOFFSET = "LeftGloveOffset";
    private const string ANTIPIRACY = "AntiPiracyAcknowledgement";
    private const string REDUCEMOTION = "ReduceMotion";
    private const string PLAYERHEIGHT = "PLAYER_HEIGHT";
    private const string MASTERVOLUME = "MasterVolume";
    private const string MUSICVOLUME = "MusicVolume";
    private const string SFXVOLUME = "SFXVolume";
    
    private void Start()
    {
        if (SettingsManager.GetSetting(PLAYEDBEFORE, false, false))
        {
            if (ProfileManager.Instance != null)
            {
                ProfileManager.Instance.activeProfileUpdated.AddListener(GetOldSettings);
            }
        }
        else
        {
            Destroy(this);
        }
    }

    private void GetOldSettings()
    {
        if (ProfileManager.Instance.ActiveProfile == null)
        {
            return;
        }
        
        ProfileManager.Instance.activeProfileUpdated.RemoveListener(GetOldSettings);
        
        var useMeters = SettingsManager.GetSetting(USEMETERS, 0, false);
        var customColorCount = SettingsManager.GetSetting(CUSTOMCOLORSETCOUNT, 0, false);
        var activeColorIndex = SettingsManager.GetSetting(ACTIVECOLORINDEX, 0, false);
        var colors = new List<ColorsManager.ColorSet>();
        for (var i = 0; i < customColorCount; i++)
        {
            colors.Add(SettingsManager.GetSetting($"{CUSTOMCOLORSETNUMBERX}{i}", ColorsManager.ColorSet.Default, false));
        }
        
        var rightGloveRot = SettingsManager.GetSetting(RIGHTGLOVEROTATION,SettingsManager.DEFAULTGLOVEROTATION, false);
        var leftGloveRot = SettingsManager.GetSetting(LEFTGLOVEROTATION,SettingsManager.DEFAULTGLOVEROTATION, false);
        var rightGloveOffset = SettingsManager.GetSetting(RIGHTGLOVEOFFSET,Vector3.zero, false);
        var leftGloveOffset = SettingsManager.GetSetting(LEFTGLOVEOFFSET,Vector3.zero, false);
        
        var playerHeight = SettingsManager.GetSetting(PLAYERHEIGHT, 0f, false);
        
        var leftHanded = SettingsManager.GetSetting(LEFTHANDED, false, false);
        var reduceMotion = SettingsManager.GetSetting(REDUCEMOTION, false, false);
        var antiPiracy = SettingsManager.GetSetting(ANTIPIRACY, false, false);

        var masterVolume = SettingsManager.GetSetting(MASTERVOLUME, 1f, false);
        var musicVolume = SettingsManager.GetSetting(MUSICVOLUME, 1f, false);
        var sfxVolume = SettingsManager.GetSetting(SFXVOLUME, 1f, false);
        
        SettingsManager.SetSetting(USEMETERS, useMeters);
        SettingsManager.SetSetting(ACTIVECOLORINDEX, activeColorIndex);
        SettingsManager.SetSetting(CUSTOMCOLORS, colors);
        SettingsManager.SetSetting(RIGHTGLOVEOFFSET, rightGloveOffset);
        SettingsManager.SetSetting(LEFTGLOVEOFFSET, leftGloveOffset);
        SettingsManager.SetSetting(RIGHTGLOVEROTATION, rightGloveRot);
        SettingsManager.SetSetting(LEFTGLOVEROTATION, leftGloveRot);
        SettingsManager.SetSetting(PLAYERHEIGHT, playerHeight);
        SettingsManager.SetSetting(LEFTHANDED, leftHanded);
        SettingsManager.SetSetting(REDUCEMOTION, reduceMotion);
        SettingsManager.SetSetting(ANTIPIRACY,antiPiracy);
        SettingsManager.SetSetting(MASTERVOLUME, masterVolume);
        SettingsManager.SetSetting(MUSICVOLUME, musicVolume);
        SettingsManager.SetSetting(SFXVOLUME, sfxVolume);
        
        
        ClearOldSettings(customColorCount);
        
        ProfileManager.Instance.ActiveProfileUpdated();
    }

    private static void ClearOldSettings(int customColorCount)
    {
        SettingsManager.DeleteSetting(PLAYEDBEFORE, false);
        SettingsManager.DeleteSetting(USEMETERS,  false);
        SettingsManager.DeleteSetting(CUSTOMCOLORSETCOUNT,  false);
        SettingsManager.DeleteSetting(ACTIVECOLORINDEX,  false);
        
        for (var i = 0; i < customColorCount; i++)
        {
            SettingsManager.DeleteSetting($"{CUSTOMCOLORSETNUMBERX}{i}", false);
        }
        
        SettingsManager.DeleteSetting(RIGHTGLOVEROTATION, false);
        SettingsManager.DeleteSetting(LEFTGLOVEROTATION,false);
        SettingsManager.DeleteSetting(RIGHTGLOVEOFFSET, false);
        SettingsManager.DeleteSetting(LEFTGLOVEOFFSET, false);
        
        SettingsManager.DeleteSetting(PLAYERHEIGHT, false);
        SettingsManager.DeleteSetting(LEFTHANDED, false);
        SettingsManager.DeleteSetting(REDUCEMOTION,  false);
        SettingsManager.DeleteSetting(ANTIPIRACY,  false);

        SettingsManager.DeleteSetting(MASTERVOLUME,  false);
        SettingsManager.DeleteSetting(MUSICVOLUME,  false);
        SettingsManager.DeleteSetting(SFXVOLUME,  false);
    }
}
