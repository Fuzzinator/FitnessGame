using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SetRendererFeatures : MonoBehaviour
{
    [SerializeField]
    private bool _enabledOnEnable;
    [SerializeField]
    private bool _enabledOnDisable;
    [SerializeField]
    private UniversalRenderPipelineAsset _settings;

    private void OnEnable()
    {
        ToggleDepthTexture(_enabledOnEnable);
    }

    private void OnDisable()
    {
        ToggleDepthTexture(_enabledOnDisable);
    }

    public void ToggleDepthTexture(bool enable)
    {
        _settings.supportsCameraDepthTexture = enable;
    }
}
