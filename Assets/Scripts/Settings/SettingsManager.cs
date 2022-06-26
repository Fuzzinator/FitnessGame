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

    private static readonly string[] _volumeNames = new[] {MASTERVOLUME, MUSICVOLUME, SFXVOLUME};

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
        return GetFloatSetting(_volumeNames[(int) mixer]);
    }

    public void SaveVolumeMixer(VolumeMixer mixer, float value)
    {
        SetSetting(_volumeNames[(int) mixer], value);
    }

    private void SetAudioSettings()
    {
        var musicVolume = GetFloatSetting(MUSICVOLUME);
        var sfxVolume = GetFloatSetting(SFXVOLUME);
        SetVolumeMixer(VolumeMixer.Master, GetFloatSetting(MASTERVOLUME));
        SetVolumeMixer(VolumeMixer.Music, musicVolume);
        SetVolumeMixer(VolumeMixer.SFX, sfxVolume);
    }

    public static string GetVolumeMixerName(VolumeMixer mixer)
    {
        return _volumeNames[(int) mixer];
    }

    #endregion


    public static float GetFloatSetting(string settingName, float defaultValue = 1f)
    {
        return ES3.Load(settingName, defaultValue);
    }

    /*public static void SetFloatSetting(string settingName, float value)
    {
        ES3.Save(settingName, value);
    }*/

    public static void SetSetting<T>(string settingName, T value) where T : struct
    {
        ES3.Save(settingName, value);
    }

    public static T GetSetting<T>(string settingName, T defaultValue) where T :struct
    {
        return ES3.Load(settingName, defaultValue);
    }
}

public enum VolumeMixer
{
    Master = 0,
    Music = 1,
    SFX = 2
}