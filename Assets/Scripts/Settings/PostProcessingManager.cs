using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager : MonoBehaviour
{
    public static PostProcessingManager Instance { get; private set; }
    [SerializeField]
    private VolumeProfile _profile;

    private Bloom _bloom = null;

    private const string UsePostProcessing = "UsePostProcessing";

    public bool IsEnabledDefault
    {
        get
        {
#if UNITY_ANDROID
            return OVRPlugin.GetSystemHeadsetType() != OVRPlugin.SystemHeadset.Oculus_Quest;
#else
            return true;
#endif
        }
    }

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
        SetUp();
    }
    private void SetUp()
    {
        _profile.TryGet(out _bloom);
        var enable = SettingsManager.GetSetting(UsePostProcessing, IsEnabledDefault);
        UpdateBloom(enable);
    }

    public void UpdateBloom(bool enable)
    {
        if (_bloom == null)
        {
            return;
        }
        _bloom.active = enable;
    }
}
