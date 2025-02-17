using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static UnityEngine.PostProcessing.AmbientOcclusionModel;

public class PostProcessingManager : MonoBehaviour
{
    public static PostProcessingManager Instance { get; private set; }
    [SerializeField]
    private VolumeProfile _profile;
    [SerializeField]
    private UniversalRenderPipelineAsset _renderPipeline;

    private Bloom _bloom = null;

    private const string UsePostProcessing = "UsePostProcessing";
    private const string EnableAntiAliasing = "EnableAntiAliasing";


#if UNITY_ANDROID
    public OVRPlugin.SystemHeadset CurrentHeadset { get; private set; }
#endif

    public bool IsQuest1
    {
        get
        {
#if UNITY_ANDROID
            if (CurrentHeadset == OVRPlugin.SystemHeadset.None)
            {
                CurrentHeadset = OVRPlugin.GetSystemHeadsetType();
            }
            return CurrentHeadset == OVRPlugin.SystemHeadset.Oculus_Quest;
#else
            return false;
#endif
        }
    }
    public bool AllowAntiAliasing
    {
        get
        {
#if UNITY_ANDROID
            if (CurrentHeadset == OVRPlugin.SystemHeadset.None)
            {
                CurrentHeadset = OVRPlugin.GetSystemHeadsetType();
            }
            return CurrentHeadset != OVRPlugin.SystemHeadset.Oculus_Quest && CurrentHeadset != OVRPlugin.SystemHeadset.Oculus_Quest_2;
#else
            return true;
#endif
        }
    }

    public bool AntiAliasingEnabled { get; private set; }

    public UnityEvent<bool> AntiAliasingStateChanged { get; private set; }

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
        AntiAliasingStateChanged = new UnityEvent<bool>();
#if UNITY_ANDROID
        CurrentHeadset = OVRPlugin.GetSystemHeadsetType();
#endif
        SetUp();
    }

    private void OnDestroy()
    {
        ProfileManager.Instance?.activeProfileUpdated.RemoveListener(SetUp);
    }

    private void SetUp()
    {
        if (_bloom == null)
        {
            _profile.TryGet(out _bloom);
        }

        var enable = SettingsManager.GetSetting(UsePostProcessing, IsQuest1);

        var aaSetting = SettingsManager.GetSetting(EnableAntiAliasing, AllowAntiAliasing);

        UpdateBloom(enable);

        UpdateAntiAliasing(AntiAliasingEnabled);

        ProfileManager.Instance?.activeProfileUpdated.AddListener(SetUp);
    }

    public void UpdateBloom(bool enable)
    {
        if (_bloom == null)
        {
            return;
        }
        _bloom.active = enable;
    }

    public void UpdateAntiAliasing(bool enableAA)
    {
        var previousAA = AntiAliasingEnabled;

        AntiAliasingEnabled = enableAA;

        if (previousAA != AntiAliasingEnabled)
        {
            AntiAliasingStateChanged?.Invoke(AntiAliasingEnabled);
        }

        SetRenderScale(AntiAliasingEnabled);
    }

    private void SetRenderScale(bool allowScaleChange)
    {
#if UNITY_ANDROID
        var scale = 1f;
        if (allowScaleChange && AllowAntiAliasing)
        {
            scale = 1.25f;

        }
#else
        var scale = allowScaleChange ? 1.5f : 1f;
#endif
        _renderPipeline.renderScale = scale;
        XRSettings.eyeTextureResolutionScale = _renderPipeline.renderScale;
    }


}
