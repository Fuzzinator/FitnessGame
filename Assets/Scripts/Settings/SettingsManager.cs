using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using static SettingsManager;
using static UnityEngine.Rendering.DebugUI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [SerializeField]
    private AudioMixer _mixer;

    private static float[] _volumes = new float[5];

    private static Dictionary<string, AsyncReactiveProperty<bool>> _boolSettings;
    private static Dictionary<string, AsyncReactiveProperty<float>> _floatSettings;
    private static Dictionary<string, AsyncReactiveProperty<int>> _intSettings;
    private static Dictionary<string, AsyncReactiveProperty<Vector3>> _vec3Settings;
    private static Dictionary<string, AsyncReactiveProperty<Quaternion>> _quaternionSettings;

    //public static readonly Quaternion DEFAULTGLOVEROTATION =
#if UNITY_ANDROID// && !UNITY_EDITOR //Oculus Quest 2
    private static readonly Quaternion Quest2Rotation = new Quaternion(0.18f, 0f, 0f, 0.98f);
#elif UNITY_STANDALONE_WIN
    private static readonly Quaternion ViveWandRotation = new Quaternion(.615f, 0f, 0f, .8f);
    private static readonly Quaternion OculusTouchRotation = new Quaternion(0.7f, 0f, 0f, 0.71f);
    private static readonly Quaternion IndexRotation = new Quaternion(0.707106829f, 0, 0, 0.707106829f);
#endif
    public static UnityEvent<string, bool> CachedBoolSettingsChanged { get; private set; } = new UnityEvent<string, bool>();
    public static UnityEvent<string, int> CachedIntSettingsChanged { get; private set; } = new UnityEvent<string, int>();
    public static UnityEvent<string, float> CachedFloatSettingsChanged { get; private set; } = new UnityEvent<string, float>();
    public static UnityEvent<string, Vector3> CachedVector3SettingsChanged { get; private set; } = new UnityEvent<string, Vector3>();
    public static UnityEvent<string, Quaternion> CachedQuaternionSettingsChanged { get; private set; } = new UnityEvent<string, Quaternion>();

    #region Const Strings

    private const string MASTERVOLUME = "MasterVolume";
    private const string MUSICVOLUME = "MusicVolume";
    private const string SFXVOLUME = "SFXVolume";

    private const string MENUMUSICVOLUME = "MenuMusicVolume";
    private const string MENUSFXVOLUME = "MenuSFXVolume";

    public const string AverageArmLength = "AverageArmLength";

    public const string LEFTGLOVEOFFSET = "LeftGloveOffset";
    public const string LEFTGLOVEROTOFFSET = "LeftGloveRotationOffset";
    public const string RIGHTGLOVEOFFSET = "RightGloveOffset";
    public const string RIGHTGLOVEROTOFFSET = "RightGloveRotationOffset";

    public const string REDUCEMOTION = "ReduceMotion";

    private const string FPSSETTING = "TargetFrameRate";

    private const string HTCViveWand = "HTC Vive Controller";
    private const string KronosController = "KHR Simple Controller";
    private const string WMRController = "Windows MR Controller";
    private const string OculusTouchController = "Oculus Touch Controller";
    private const string IndexController = "Index Controller";

    public const string UseAdaptiveStrikeMode = "AdaptiveStrikeMode";
    public const string MinHitSpeed = "MinHitSpeed";

    public const string PassthroughInMenu = "PassthroughInMainMenu";

    public const string SearchFilter = "SearchFilter:";

    private static readonly string[] _volumeNames = new[] { MASTERVOLUME, MUSICVOLUME, SFXVOLUME, MENUMUSICVOLUME, MENUSFXVOLUME };

    #endregion

    public static float CurrentMinHitSpeed => GetCachedFloat(MinHitSpeed, DefaultMinHitSpeed);
    public static float DefaultMinHitSpeed = 1.5f;
    public static float DefaultMaxHitSpeed = 10f;
    public static float SuperStrikeHitSpeed => CurrentMinHitSpeed + 1.5f;

    public static bool UseEnlongatedCollider;
    public static bool UseFixedHitDirection;

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

        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.activeProfileUpdated.AddListener(ProfileChanged);
        }
        else
        {
            WaitForProfileManager().Forget();
        }
    }

    private async UniTaskVoid WaitForProfileManager()
    {
        await UniTask.WaitUntil(() => ProfileManager.Instance != null);
        ProfileManager.Instance.activeProfileUpdated.AddListener(ProfileChanged);
    }

    private void ProfileChanged()
    {
        ClearCachedValues();
        SetInitialProfileSettings();
    }

    private void SetInitialProfileSettings()
    {
        SetAudioSettings();

        var fps = GetSetting(FPSSETTING, (int)GetFPSSetting());
        SetTargetFPS((FPSSetting)fps);
    }

    #region Audio Settings

    public float GetVolumeMixer(VolumeMixer mixer)
    {
        //_mixer.GetFloat(_volumeNames[(int) mixer], out var value);
        //value = Mathf.Pow(10, value);
        return _volumes[(int)mixer]; //value;
    }

    public void SetVolumeMixer(VolumeMixer mixer, float value, bool autoSave = false)
    {
        _volumes[(int)mixer] = value;

        var convertedValue = value == 0 ? -80 : Mathf.Log10(value) * 20;
        switch (mixer)
        {
            case VolumeMixer.Master:
                _mixer.SetFloat(MASTERVOLUME, convertedValue);
                break;
            case VolumeMixer.Music:
                _mixer.SetFloat(MUSICVOLUME, convertedValue);
                break;
            case VolumeMixer.SFX:
                _mixer.SetFloat(SFXVOLUME, convertedValue);
                break;
            case VolumeMixer.MenuMusic:
                _mixer.SetFloat(MENUMUSICVOLUME, convertedValue);
                break;
            case VolumeMixer.MenuSFX:
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
        return GetSetting(_volumeNames[(int)mixer], mixer < VolumeMixer.MenuMusic ? 1f : .5f);
    }

    public void SaveVolumeMixer(VolumeMixer mixer, float value)
    {
        SetSetting(_volumeNames[(int)mixer], value);
    }

    private void SetAudioSettings()
    {
        var masterVolume = GetSetting(MASTERVOLUME, 1f);
        var musicVolume = GetSetting(MUSICVOLUME, 1f);
        var sfxVolume = GetSetting(SFXVOLUME, 1f);
        var menuMusicVolume = GetSetting(MENUMUSICVOLUME, .5f);
        var menuSfxVolume = GetSetting(MENUSFXVOLUME, .5f);

        SetVolumeMixer(VolumeMixer.Master, masterVolume);
        SetVolumeMixer(VolumeMixer.Music, musicVolume);
        SetVolumeMixer(VolumeMixer.SFX, sfxVolume);
        SetVolumeMixer(VolumeMixer.MenuMusic, menuMusicVolume);
        SetVolumeMixer(VolumeMixer.MenuSFX, menuSfxVolume);
    }

    public static string GetVolumeMixerName(VolumeMixer mixer)
    {
        return _volumeNames[(int)mixer];
    }

    #endregion

    public static void ClearCachedValues()
    {
        _boolSettings?.Clear();
        _floatSettings?.Clear();
        _intSettings?.Clear();
        _vec3Settings?.Clear();
        _quaternionSettings?.Clear();
    }

    public static void SetCachedBool(string settingName, bool value, Profile overrideProfile = null)
    {
        CacheBool(settingName, value);
        SetSetting(settingName, value, true, overrideProfile);
    }

    public static void SetCachedFloat(string settingName, float value, Profile overrideProfile = null)
    {
        CacheFloat(settingName, value);
        SetSetting(settingName, value, true, overrideProfile);
    }

    public static void SetCachedInt(string settingName, int value, Profile overrideProfile = null)
    {
        CacheInt(settingName, value);
        SetSetting(settingName, value, true, overrideProfile);
    }
    public static void SetCachedVector3(string settingName, Vector3 value, Profile overrideProfile = null)
    {
        CacheVector3(settingName, value);
        SetSetting(settingName, value, true, overrideProfile);
    }
    public static void SetCacheQuaternion(string settingName, Quaternion value, Profile overrideProfile = null)
    {
        CacheQuaternion(settingName, value);
        SetSetting(settingName, value, true, overrideProfile);
    }

    private static void CacheBool(string settingName, bool value)
    {
        _boolSettings ??= new Dictionary<string, AsyncReactiveProperty<bool>>();
        if (!_boolSettings.ContainsKey(settingName))
        {
            _boolSettings[settingName] = new AsyncReactiveProperty<bool>(value);
            CachedBoolSettingsChanged.Invoke(settingName, value);
        }
        _boolSettings[settingName].Value = value;
    }

    private static void CacheFloat(string settingName, float value)
    {
        _floatSettings ??= new Dictionary<string, AsyncReactiveProperty<float>>();
        if (!_floatSettings.ContainsKey(settingName))
        {
            _floatSettings[settingName] = new AsyncReactiveProperty<float>(value);
            CachedFloatSettingsChanged.Invoke(settingName, value);
        }
        _floatSettings[settingName].Value = value;
    }

    private static void CacheInt(string settingName, int value)
    {
        _intSettings ??= new Dictionary<string, AsyncReactiveProperty<int>>();
        if (!_intSettings.ContainsKey(settingName))
        {
            _intSettings[settingName] = new AsyncReactiveProperty<int>(value);
            CachedIntSettingsChanged.Invoke(settingName, value);
        }
        _intSettings[settingName].Value = value;
    }

    private static void CacheVector3(string settingName, Vector3 value)
    {
        _vec3Settings ??= new Dictionary<string, AsyncReactiveProperty<Vector3>>();
        if (!_vec3Settings.ContainsKey(settingName))
        {
            _vec3Settings[settingName] = new AsyncReactiveProperty<Vector3>(value);
            CachedVector3SettingsChanged.Invoke(settingName, value);
        }
        _vec3Settings[settingName].Value = value;
    }

    private static void CacheQuaternion(string settingName, Quaternion value)
    {
        _quaternionSettings ??= new Dictionary<string, AsyncReactiveProperty<Quaternion>>();
        if (!_quaternionSettings.ContainsKey(settingName))
        {
            _quaternionSettings[settingName] = new AsyncReactiveProperty<Quaternion>(value);
            CachedQuaternionSettingsChanged.Invoke(settingName, value);
        }
        _quaternionSettings[settingName].Value = value;
    }

    public static bool GetCachedBool(string settingName, bool defaultValue, Profile overrideProfile = null)
    {
        if (_boolSettings != null && _boolSettings.TryGetValue(settingName, out var cachedValue))
        {
            return cachedValue;
        }

        var setting = GetSetting(settingName, defaultValue, true, overrideProfile);
        CacheBool(settingName, setting);
        return setting;
    }

    public static float GetCachedFloat(string settingName, float defaultValue, Profile overrideProfile = null)
    {
        if (_floatSettings != null && _floatSettings.TryGetValue(settingName, out var cachedValue))
        {
            return cachedValue;
        }

        var setting = GetSetting(settingName, defaultValue, true, overrideProfile);
        CacheFloat(settingName, setting);
        return setting;
    }

    public static int GetCachedInt(string settingName, int defaultValue, Profile overrideProfile = null)
    {
        if (_intSettings != null && _intSettings.TryGetValue(settingName, out var cachedValue))
        {
            return cachedValue;
        }

        var setting = GetSetting(settingName, defaultValue, true, overrideProfile);
        CacheInt(settingName, setting);
        return setting;
    }

    public static Vector3 GetCachedVector3(string settingName, Vector3 defaultValue, Profile overrideProfile = null)
    {
        if (_vec3Settings != null && _vec3Settings.TryGetValue(settingName, out var cachedValue))
        {
            return cachedValue;
        }

        var setting = GetSetting(settingName, defaultValue, true, overrideProfile);
        CacheVector3(settingName, setting);
        return setting;
    }

    public static Quaternion GetCachedQuaternion(string settingName, Quaternion defaultValue, Profile overrideProfile = null)
    {
        if (_quaternionSettings != null && _quaternionSettings.TryGetValue(settingName, out var cachedValue))
        {
            return cachedValue;
        }

        var setting = GetSetting(settingName, defaultValue, true, overrideProfile);
        CacheQuaternion(settingName, setting);
        return setting;
    }

    public static bool TrySubscribeToCachedBool(string settingName, Action<bool> subscription)
    {
        if (_boolSettings == null || !_boolSettings.ContainsKey(settingName))
        {
            return false;
        }

        _boolSettings[settingName].Subscribe(subscription);
        return true;
    }

    public static bool TrySubscribeToCachedfloat(string settingName, Action<float> subscription)
    {
        if (_floatSettings == null || !_floatSettings.ContainsKey(settingName))
        {
            return false;
        }

        _floatSettings[settingName].Subscribe(subscription);
        return true;
    }

    public static bool TrySubscribeToCachedInt(string settingName, Action<int> subscription)
    {
        if (_intSettings == null || !_intSettings.ContainsKey(settingName))
        {
            return false;
        }

        _intSettings[settingName].Subscribe(subscription);
        return true;
    }

    public static bool TrySubscribeToCachedVector3(string settingName, Action<Vector3> subscription)
    {
        if (_vec3Settings == null || !_vec3Settings.ContainsKey(settingName))
        {
            return false;
        }

        _vec3Settings[settingName].Subscribe(subscription);
        return true;
    }

    public static bool TrySubscribeToCachedQuaternion(string settingName, Action<Quaternion> subscription)
    {
        if (_quaternionSettings == null || !_quaternionSettings.ContainsKey(settingName))
        {
            return false;
        }

        _quaternionSettings[settingName].Subscribe(subscription);
        return true;
    }

    private static void SetTargetFPS(FPSSetting defaultValue = FPSSetting.Unset)
    {
#if UNITY_ANDROID //Oculus Quest
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

    public static void SetSetting<T>(string settingName, T value, bool isProfileSetting = true, Profile overrideProfile = null)
    {
        try
        {
            if (!isProfileSetting)
            {
                ES3.Save(settingName, value);
            }
            else
            {
                if (overrideProfile != null)
                {
                    var settings = ProfileManager.GetProfileSettings(overrideProfile);
                    if (settings == null)
                    {
                        return;
                    }
                    settings.location = ES3.Location.File;

                    ES3.Save(settingName, value, settings);
                    return;
                }
                if (ProfileManager.Instance.ProfileSettings == null)
                {
                    return;
                }

                ES3.Save(settingName, value, ProfileManager.Instance.ProfileSettings);
            }
        }
        catch (Exception ex) when (ex is InvalidCastException)
        {
            AzureSqlManager.Instance.TrySendErrorReport($"Encountered error setting setting \"{settingName}\" reported error is: {ex.Message}", ex.StackTrace);
            DeleteSetting(settingName, isProfileSetting, overrideProfile);
            SetSetting(settingName, value, isProfileSetting, overrideProfile);
        }
        catch (Exception ex)
        {
            if (ex is FormatException)
            {
                HandleFormatException(overrideProfile);
                return;
            }
            Debug.LogError($"Cant set:{settingName}--{ex.Message}--{ex.StackTrace}");
        }
    }

    public static T GetSetting<T>(string settingName, T defaultValue, bool isProfileSetting = true, Profile overrideProfile = null)
    {
        try
        {
            if (!isProfileSetting)
            {
                return ES3.Load(settingName, defaultValue);
            }

            if (overrideProfile != null)
            {
                var settings = ProfileManager.GetProfileSettings(overrideProfile);
                if(settings == null)
                {
                    return defaultValue;
                }

                return ES3.Load(settingName, defaultValue, settings);
            }
            if (ProfileManager.Instance.ProfileSettings == null)
            {
                return defaultValue;
            }

            var value = ES3.Load(settingName, defaultValue, ProfileManager.Instance.ProfileSettings);

            return value;
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException)
        {
            AzureSqlManager.Instance.TrySendErrorReport($"Encountered error getting setting \"{settingName}\" reported error is: {ex.Message}", ex.StackTrace);
            DeleteSetting(settingName, isProfileSetting, overrideProfile);
            return defaultValue;
        }
        catch (Exception ex)
        {
            if (ex is FormatException)
            {
                HandleFormatException(overrideProfile);
                return defaultValue;
            }

            Debug.LogError($"Cant get:{settingName}--{ex.Message}--{ex.StackTrace}");
            return defaultValue;
        }
    }

    private static void HandleFormatException(Profile overrideProfile)
    {
        var visuals = new Notification.NotificationVisuals("It appears your profile has been corrupted. Selecting \"Confirm\" will delete your current profile and you will be prompted to make a new one.", "Profile Corrupted", "Confirm", "Cancel");
        NotificationManager.RequestNotification(visuals, () => ProfileManager.Instance.CleanUpCorruptedProfile(overrideProfile));
    }

    public static void DeleteSetting(string settingName, bool isProfileSetting = true, Profile overrideProfile = null)
    {
        if (!isProfileSetting)
        {
            ES3.DeleteKey(settingName);
        }
        else
        {
            if (overrideProfile != null)
            {
                var settings = ProfileManager.GetProfileSettings(overrideProfile);
                if (settings == null)
                {
                    return;
                }

                ES3.DeleteKey(settingName, settings);
                return;
            }
            ES3.DeleteKey(settingName, ProfileManager.Instance.ProfileSettings);
        }
    }

    public static void SetFPSSetting(int value)
    {
        SetSetting(FPSSETTING, value);
        SetTargetFPS((FPSSetting)value);
    }

    public static bool HasSetting(string settingName, bool isProfileSetting = true, Profile overrideProfile = null)
    {
        if (!isProfileSetting)
        {
            return ES3.KeyExists(settingName);
        }

        if (overrideProfile != null)
        {
            try
            {
                var settings = ProfileManager.GetProfileSettings(overrideProfile);
                if (settings == null)
                {
                    return false;
                }

                return ES3.KeyExists(settingName, settings);
            }
            catch(Exception ex)
            {
                if (ex is FormatException)
                {
                    HandleFormatException(overrideProfile);
                    throw ex;
                }
            }
            return false;
        }
        if (ProfileManager.Instance.ProfileSettings == null)
        {
            return false;
        }

        return ES3.KeyExists(settingName, ProfileManager.Instance.ProfileSettings);
    }

    public static Quaternion GetDefaultControllerRotation(string controllerName)
    {
        if (string.IsNullOrEmpty(controllerName))
        {
            return Quaternion.identity;
        }

        Debug.Log($"Controller Name is {controllerName}");
#if UNITY_ANDROID

        switch (true)
        {
            case var quest when controllerName.Contains(OculusTouchController):
                return Quest2Rotation;
            default:
                return Quaternion.identity;
        }

        /*#elif UNITY_STANDALONE_WIN
                switch (true)
                {
                    case var vive when controllerName.Contains(HTCViveWand):
                    case var oculus when controllerName.Contains(OculusTouchController):
                    case var index when controllerName.Contains(IndexController):
                        return ViveWandRotation;
                    default:
                        return Quaternion.identity;
                }*/
#else
        if(string.Equals("Oculus Touch Controller OpenXR", controllerName))
        {
            return OculusTouchRotation;
        }

        if(controllerName.Contains(IndexController))
        {
            return IndexRotation;
        }
        return Quaternion.identity;
#endif
    }

    public static FPSSetting GetFPSSetting()
    {
#if UNITY_ANDROID //Oculus
        var headset = OVRPlugin.GetSystemHeadsetType();

        return (FPSSetting)GetSetting(FPSSETTING,
            ((int)(headset == OVRPlugin.SystemHeadset.Oculus_Quest ? FPSSetting._72 : FPSSetting._90)));
#else
        return FPSSetting._90;
#endif
    }

    public static float GetMinHitSpeed(HitSideType hitSide)
    {
        var minSpeed = CurrentMinHitSpeed;
        var useAdaptive = GetCachedBool(UseAdaptiveStrikeMode, false);
        if (useAdaptive)
        {
            var average = hitSide == HitSideType.Left ? ScoringAndHitStatsManager.Instance.AverageLeftHitSpeed :
                                                        ScoringAndHitStatsManager.Instance.AverageRightHitSpeed;
            minSpeed = Mathf.Clamp(average * .75f, CurrentMinHitSpeed, DefaultMaxHitSpeed);
        }

        return minSpeed;
    }

    public static float GetSuperStrikeHitSpeed(HitSideType hitSide)
    {
        var minSpeed = SuperStrikeHitSpeed;
        var useAdaptive = GetCachedBool(UseAdaptiveStrikeMode, false);
        if (useAdaptive)
        {
            var average = hitSide == HitSideType.Left ? ScoringAndHitStatsManager.Instance.AverageLeftHitSpeed :
                                                        ScoringAndHitStatsManager.Instance.AverageRightHitSpeed;
            minSpeed = Mathf.Clamp(average + 1.5f, CurrentMinHitSpeed, DefaultMaxHitSpeed);
        }

        return minSpeed;
    }

    public static void SaveSearchFilter(string searchName, string filter)
    {
        SetSetting($"{SearchFilter}{searchName}", filter);
    }

    public static string GetSearchFilter(string searchName)
    {
        return GetSetting($"{SearchFilter}{searchName}", string.Empty);
    }

    private struct CachedBoolSetting
    {
        public bool IsSet { get; private set; }
        public bool Value { get; private set; }

        public CachedBoolSetting(bool value)
        {
            IsSet = true;
            Value = value;
        }
    }
}

public enum VolumeMixer
{
    Master = 0,
    Music = 1,
    SFX = 2,
    MenuMusic = 3,
    MenuSFX = 4
}

public enum FPSSetting
{
    Unset = -1,
    _72 = 0,
    _90 = 1,
    _120 = 2
}