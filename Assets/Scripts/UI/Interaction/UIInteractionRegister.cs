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
    
    public void SetInteractionState(bool on)
    {
        _rayInteractor.enabled = on;
        _lineRenderer.enabled = on;
        _interactorLineVisual.enabled = on;
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
