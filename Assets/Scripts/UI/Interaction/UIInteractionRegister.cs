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
    [field: SerializeField]
    public bool IsEnabled { get; private set; }
    private bool _targetOnState;
    private bool _awaiting;

    public void SetInteractionState(bool on)
    {
        _targetOnState = on;
        if (on == IsEnabled || _awaiting)
        {
            return;
        }
        WaitAndSet().Forget();
    }

    private async UniTaskVoid WaitAndSet()
    {
        _awaiting = true;
        await UniTask.DelayFrame(1);
        _awaiting = false;
        
        _rayInteractor.enableUIInteraction = _targetOnState;
        _lineRenderer.enabled = _targetOnState;
        _interactorLineVisual.enabled = _targetOnState;
        IsEnabled = _targetOnState;
    }

    /*private void Start()
    {
        Disable().Forget();
    }

    private async UniTaskVoid Disable()
    {
        await UniTask.DelayFrame(1);
        _rayInteractor.enableUIInteraction = false;
        await UniTask.DelayFrame(1);
        _rayInteractor.enableUIInteraction = true;
    }*/

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
