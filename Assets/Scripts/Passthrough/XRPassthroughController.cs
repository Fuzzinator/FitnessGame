using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class XRPassthroughController : MonoBehaviour
{
    public static XRPassthroughController Instance { get; private set; }

    [SerializeField]
    private bool _passthroughEnabled;
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private OVRPassthroughLayer _passthroughLayer;

    /// <summary>
    /// Did the _camera have the m_RenderPostProcessing set to true
    /// </summary>
    private bool _cameraAllowedPP;
    private UniversalAdditionalCameraData _cameraData;

    public bool PassthroughEnabled
    {
        get
        {
            return _passthroughEnabled;
        }
        set
        {
            _passthroughEnabled = value;
            if (_passthroughEnabled)
            {
                SetCameraToPassthrough();
            }
            else
            {
                DisableCameraPassthrough();
            }
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
        _cameraData = _camera.GetUniversalAdditionalCameraData();
        _cameraAllowedPP = _cameraData.renderPostProcessing;
        if (PassthroughManager.DynamicInstance != null)
        {
            PassthroughEnabled = PassthroughManager.DynamicInstance.isInsightPassthroughEnabled;
        }
        else
        {
            DisableCameraPassthrough();
        }
    }

    public void SetCameraToPassthrough()
    {
        //_camera.backgroundColor = new Color(0, 0, 0, 0);
        //_camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.allowHDR = false;
        _camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
        if (OVRManager.instance != null)
        {
            SetOVRManager(true);
        }
        else
        {
            AwaitAndSetOVRManager(true).Forget();
        }
        _passthroughLayer.enabled = true;
    }

    private void DisableCameraPassthrough()
    {
        //_camera.clearFlags = CameraClearFlags.Skybox;
        _camera.allowHDR = true;
        _camera.GetUniversalAdditionalCameraData().renderPostProcessing = _cameraAllowedPP;
        if (OVRManager.instance != null)
        {
            SetOVRManager(false);
        }
        else
        {
            AwaitAndSetOVRManager(false).Forget();
        }
        _passthroughLayer.enabled = false;
    }

    private async UniTaskVoid AwaitAndSetOVRManager(bool enable)
    {
        await UniTask.WaitWhile(() => OVRManager.instance == null);
        SetOVRManager(enable);
    }

    private void SetOVRManager(bool enable)
    {
        //OVRManager.instance.enabled = enable;
        OVRManager.instance.isInsightPassthroughEnabled = enable;
    }
}
