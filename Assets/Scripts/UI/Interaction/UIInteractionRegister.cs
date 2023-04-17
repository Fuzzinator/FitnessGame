using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
    private CancellationToken _cancellationToken;

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
        await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
        if(_cancellationToken.IsCancellationRequested)
        {
            return;
        }

        _awaiting = false;
        
        _rayInteractor.enableUIInteraction = _targetOnState;
        _lineRenderer.enabled = _targetOnState;
        _interactorLineVisual.enabled = _targetOnState;
        IsEnabled = _targetOnState;
    }

    private void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
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
