using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PassthroughController : MonoBehaviour
{
    public static PassthroughController Instance { get; private set; }

    [SerializeField]
    private bool _passthroughEnabled;

    public bool PassthroughEnabled
    {
        get 
        {
#if UNITY_ANDROID
            return _passthroughEnabled && _passthroughInitializationState == PassthroughInitializationState.Initialized;
#else
            return false;
#endif
        }
    }

    private static PassthroughInitializationState _passthroughInitializationState = PassthroughInitializationState.Unspecified;
    private CancellationToken _cancellationToken;
    private PassthroughLayer _passthroughLayer;

    private void Awake()
    {
        if(Instance == null) 
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
        _cancellationToken = this.GetCancellationTokenOnDestroy();
        if(_passthroughEnabled && !PassthroughInitializedOrPending(_passthroughInitializationState))
        {
            InitializePassthrough();
            SetCameraToPassthrough(_passthroughLayer);
        }
        else if(!_passthroughEnabled) 
        {
            DisablePassthrough();
            SetCameraNotPassthrough();
        }
    }

    private void OnDestroy()
    {
        DisablePassthrough();
    }

    #region OVR Passthrough
    private bool InitializePassthrough()
    {
        var passthroughResult = OVRPlugin.InitializeInsightPassthrough();
        UpdateInsightPassthrough();
        return PassthroughInitializedOrPending(_passthroughInitializationState);
    }
    private void DisablePassthrough()
    {
        if (PassthroughInitializedOrPending(_passthroughInitializationState))
        {
            if (OVRPlugin.ShutdownInsightPassthrough())
            {
                _passthroughInitializationState = PassthroughInitializationState.Unspecified;
            }
            else
            {
                // If it did not shut down, it may already be deinitialized.
                bool isInitialized = OVRPlugin.IsInsightPassthroughInitialized();
                if (isInitialized)
                {
                    Debug.LogError("Failed to shut down passthrough. It may be still in use.");
                }
                else
                {
                    _passthroughInitializationState = PassthroughInitializationState.Unspecified;
                }
            }
        }
        else
        {
            // Allow initialization to proceed on restart.
            _passthroughInitializationState = PassthroughInitializationState.Unspecified;
        }
    }
    private void UpdateInsightPassthrough()
    {
        var result = OVRPlugin.GetInsightPassthroughInitializationState();
        if (result < 0)
        {
            _passthroughInitializationState = PassthroughInitializationState.Failed;
            Debug.LogError("Failed to initialize Insight Passthrough. Passthrough will be unavailable. Error " + result.ToString() + ".");
        }
        else
        {
            if (result == OVRPlugin.Result.Success_Pending)
            {
                _passthroughInitializationState = PassthroughInitializationState.Pending;
                AwaitInitializePending().Forget();
            }
            else
            {
                _passthroughInitializationState = PassthroughInitializationState.Initialized;
            }
        }
    }
    private static bool PassthroughInitializedOrPending(PassthroughInitializationState state)
    {
        return state == PassthroughInitializationState.Pending || state == PassthroughInitializationState.Initialized;
    }
    private async UniTaskVoid AwaitInitializePending()
    {
        await UniTask.WaitWhile(() => _passthroughInitializationState == PassthroughInitializationState.Pending);
        UpdateInsightPassthrough();
    }
    enum PassthroughInitializationState
    {
        Unspecified,
        Pending,
        Initialized,
        Failed
    };
    #endregion

    public void SetCameraToPassthrough(PassthroughLayer layer)
    {
        if(layer != null)
        {
            _passthroughLayer = layer;
        }
        if(_passthroughLayer != null || Head.Instance.TryGetComponent(out _passthroughLayer))
        {
            _passthroughLayer.enabled = true;
        }

        Head.Instance.HeadCamera.backgroundColor = new Color(0,0,0,0);
        Head.Instance.HeadCamera.clearFlags = CameraClearFlags.SolidColor;
    }

    private void SetCameraNotPassthrough()
    {
        if (_passthroughLayer != null || Head.Instance.TryGetComponent(out _passthroughLayer))
        {
            _passthroughLayer.enabled = false;
        }
        Head.Instance.HeadCamera.clearFlags = CameraClearFlags.Skybox;
    }
}
