using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [SerializeField]
    private AudioMixer _mixer;

    public static SettingsManager Instance { get; private set; }

    private static float[] _volumes = new float[3];

    #region Const Strings

    private const string MASTERVOLUME = "MasterVolume";
    private const string MUSICVOLUME = "MusicVolume";
    private const string SFXVOLUME = "SFXVolume";

    private const string MENUMUSICVOLUME = "MenuMusicVolume";
    private const string MENUSFXVOLUME = "MenuSFXVolume";

    public const string LEFTGLOVEOFFSET = "LeftGloveOffset";
    public const string LEFTGLOVEROTOFFSET = "LeftGloveRotationOffset";
    public const string RIGHTGLOVEOFFSET = "RightGloveOffset";
    public const string RIGHTGLOVEROTOFFSET = "RightGloveRotationOffset";

    public const string REDUCEMOTION = "ReduceMotion";

    private const string FPSSETTING = "TargetFrameRate";

    private static readonly string[] _volumeNames = new[] {MASTERVOLUME, MUSICVOLUME, SFXVOLUME};
    
    public static readonly Quaternion DEFAULTGLOVEROTATION =
#if UNITY_ANDROID && !UNITY_EDITOR//Oculus Quest 2
        new Quaternion(0.173648164f,0f,0f,0.984807789f);
#elif UNITY_EDITOR
        Quaternion.identity;
#elif UNITY_STANDALONE_WIN
        Quaternion.identity;
#else
        Quaternion.identity;
#endif
    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        SetAudioSettings();
        SetTargetFPS();
    }

    #region Audio Settings

    public float GetVolumeMixer(VolumeMixer mixer)
    {
        //_mixer.GetFloat(_volumeNames[(int) mixer], out var value);
        //value = Mathf.Pow(10, value);
        return _volumes[(int) mixer]; //value;
    }

    public void SetVolumeMixer(VolumeMixer mixer, float value, bool autoSave = false)
    {
        _volumes[(int) mixer] = value;

        var convertedValue = value == 0 ? -80 : Mathf.Log10(value) * 20;
        switch (mixer)
        {
            case VolumeMixer.Master:
                _mixer.SetFloat(MASTERVOLUME, convertedValue);
                break;
            case VolumeMixer.Music:
                _mixer.SetFloat(MUSICVOLUME, convertedValue);
                _mixer.SetFloat(MENUMUSICVOLUME, convertedValue);
                break;
            case VolumeMixer.SFX:
                _mixer.SetFloat(SFXVOLUME, convertedValue);
                _mixer.SetFloat(MENUSFXVOLUME, convertedValue);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mixer), mixer, null);
        }

        if (autoSave)
        {
            SaveVolumeMixer(mixer, value);
        }
    }

    public float LoadVolumeMixerValue(VolumeMixer mixer)
    {
        return GetSetting(_volumeNames[(int) mixer], 1f);
    }

    public void SaveVolumeMixer(VolumeMixer mixer, float value)
    {
        SetSetting(_volumeNames[(int) mixer], value);
    }

    private void SetAudioSettings()
    {
        var musicVolume = GetSetting(MUSICVOLUME, 1f);
        var sfxVolume = GetSetting(SFXVOLUME, 1f);
        SetVolumeMixer(VolumeMixer.Master, GetSetting(MASTERVOLUME, 1f));
        SetVolumeMixer(VolumeMixer.Music, musicVolume);
        SetVolumeMixer(VolumeMixer.SFX, sfxVolume);
    }

    public static string GetVolumeMixerName(VolumeMixer mixer)
    {
        return _volumeNames[(int) mixer];
    }

    #endregion


    private static void SetTargetFPS(FPSSetting defaultValue = FPSSetting.Unset)
    {
#if UNITY_ANDROID//Oculus Quest
        if (defaultValue == FPSSetting.Unset)
        {
            defaultValue = GetFPSSetting();
        }

        var actualTarget = defaultValue switch
        {
            FPSSetting._72 => 72f,
            FPSSetting._90 => 90f,
            FPSSetting._120 => 120f,
            _ => 72f
        };

        OVRPlugin.systemDisplayFrequency = actualTarget;
        OVRPlugin.occlusionMesh = true;
        
#elif UNITY_STANDALONE_WIN
        Application.targetFrameRate = defaultValue switch
        {
            FPSSetting._72 => 72,
            FPSSetting._90 => 90,
            FPSSetting._120 => 120,
            _ => 72
        };
#endif
    }

    /*public static void SetFloatSetting(string settingName, float value)
    {
        ES3.Save(settingName, value);
    }*/

    public static void SetSetting<T>(string settingName, T value, bool isProfileSetting = true)
    {
        if (!isProfileSetting)
        {
            ES3.Save(settingName, value);
        }
        else
        {
            if (ProfileManager.Instance.ProfileSettings == null)
            {
                return;
            }

            ES3.Save(settingName, value, ProfileManager.Instance.ProfileSettings);
        }
    }

    public static T GetSetting<T>(string settingName, T defaultValue, bool isProfileSetting = true)
    {
        if (!isProfileSetting)
        {
            return ES3.Load<T>(settingName, defaultValue);
        }
        
        if (ProfileManager.Instance.ProfileSettings == null)
        {
            return defaultValue;
        }
        
        return ES3.Load<T>(settingName, defaultValue, ProfileManager.Instance.ProfileSettings);
    }

    public static void SetFPSSetting(int value)
    {
        SetSetting(FPSSETTING, value);
        SetTargetFPS((FPSSetting) value);
    }

    public static FPSSetting GetFPSSetting()
    {
#if UNITY_ANDROID//Oculus
        var headset = OVRPlugin.GetSystemHeadsetType();

        return GetSetting(FPSSETTING,
            (headset == OVRPlugin.SystemHeadset.Oculus_Quest ? FPSSetting._72 : FPSSetting._90), false);
#endif
        return FPSSetting._90;
    }
}

public enum VolumeMixer
{
    Master = 0,
    Music = 1,
    SFX = 2
}

public enum FPSSetting
{
    Unset = -1,
    _72 = 0,
    _90 = 1,
    _120 = 2
}