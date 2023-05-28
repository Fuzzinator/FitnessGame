using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckForPassthrough : MonoBehaviour
{
    [SerializeField]
    private PassthroughLayer _passthroughLayer;
    private void OnEnable()
    {
        var usePassthrough = PassthroughController.Instance != null &&
                             PassthroughController.Instance.PassthroughEnabled;
        if(usePassthrough)
        {
            PassthroughController.Instance.SetCameraToPassthrough(_passthroughLayer);
        }
    }

    private void OnDisable()
    {
        _passthroughLayer.enabled = false;
    }
}
