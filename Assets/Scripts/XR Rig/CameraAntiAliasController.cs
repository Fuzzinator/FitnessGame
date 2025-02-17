using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraAntiAliasController : MonoBehaviour
{
    [SerializeField]
    private UniversalAdditionalCameraData _data;

    private const AntialiasingMode Enabled = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
    private const AntialiasingMode Disabled = AntialiasingMode.None;


    private void OnValidate()
    {
        var camera = GetComponent<Camera>();
        _data = camera.GetUniversalAdditionalCameraData();
    }


    private void OnEnable()
    {
        if(PostProcessingManager.Instance == null)
        {
            return;
        }

        var aaEnabled = PostProcessingManager.Instance.AntiAliasingEnabled;
        SetAntiAliasing(aaEnabled);

        PostProcessingManager.Instance.AntiAliasingStateChanged.AddListener(SetAntiAliasing);
    }

    private void OnDisable()
    {
        if (PostProcessingManager.Instance == null)
        {
            return;
        }

        PostProcessingManager.Instance.AntiAliasingStateChanged.RemoveListener(SetAntiAliasing);
    }

    private void SetAntiAliasing(bool enabled)
    {
        _data.antialiasing = enabled ? Enabled : Disabled;
        _data.antialiasingQuality = GetQualitySetting(enabled);
    }

    private AntialiasingQuality GetQualitySetting(bool enabled)
    {
        if(!enabled)
        {
            return AntialiasingQuality.Low;
        }
#if UNITY_ANDROID

        if (PostProcessingManager.Instance == null)
        {
            return AntialiasingQuality.Low;
        }

        return PostProcessingManager.Instance.CurrentHeadset switch
        {
            OVRPlugin.SystemHeadset.Oculus_Quest => AntialiasingQuality.Low,
            OVRPlugin.SystemHeadset.Oculus_Quest_2 => AntialiasingQuality.Medium,
            _ => AntialiasingQuality.High,
        };
#else
        return AntialiasingQuality.High;
#endif
    }
}
