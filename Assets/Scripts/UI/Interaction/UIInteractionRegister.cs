using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class UIInteractionRegister : MonoBehaviour
{
    [SerializeField]
    private bool _isRightHand;
    [SerializeField]
    private XRRayInteractor _rayInteractor;
    [SerializeField]
    private LineRenderer _lineRenderer;
    [SerializeField]
    private XRInteractorLineVisual _interactorLineVisual;
    [SerializeField]
    private bool _disable;
    [SerializeField]
    private bool _isDisabled;
    
    public void SetInteractionState(bool on)
    {
        _rayInteractor.enabled = on;
        _lineRenderer.enabled = on;
        _interactorLineVisual.enabled = on;
    }

    private void Start()
    {
        Disable().Forget();
    }

    private async UniTaskVoid Disable()
    {
        await UniTask.DelayFrame(1);
        _rayInteractor.enableUIInteraction = false;
        await UniTask.DelayFrame(1);
        _rayInteractor.enableUIInteraction = true;
        /*while (!_disable)
        {
            await UniTask.DelayFrame(1);
        }
        if (_disable && !_isDisabled)
        {
            _isDisabled = true;
            _rayInteractor.enableUIInteraction = false;
        }*/
    }

    private void OnEnable()
    {
        if (UIStateManager.Instance != null)
        {
            UIStateManager.Instance.RegisterController(this, _isRightHand);
        }
    }

    private void OnDisable()
    {
        if (UIStateManager.Instance != null)
        {
            UIStateManager.Instance.DeRegisterController(this, _isRightHand);
        }
    }
}
