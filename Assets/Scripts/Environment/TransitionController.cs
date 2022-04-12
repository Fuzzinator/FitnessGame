using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class TransitionController : MonoBehaviour
{
    [SerializeField]
    private Material _sourceMaterial;

    [SerializeField]
    private string _propertyName;

    [SerializeField]
    private float _transitionSpeed = 1;
    [SerializeField]
    private float _targetValue = 1;
    [SerializeField]
    private float _defaultValue = .65f;

    [SerializeField]
    private AnimationCurve _transitionCurve;
    
    [SerializeField]
    private UnityEvent _transitionStarted = new UnityEvent();
    [SerializeField]
    private UnityEvent _transitionCompleted = new UnityEvent();
    
    [SerializeField]
    private GameState _resumedState;

    private int _propertyID;
    private CancellationToken _cancellationToken;
    private Func<bool> _reset;

    private void Start()
    {
        _propertyID = Shader.PropertyToID(_propertyName);
        _cancellationToken = this.GetCancellationTokenOnDestroy();
        Application.quitting += ResetMaterial;
    }

    public void RequestTransition()
    {
        GameStateManager.Instance.SetState(_resumedState);
        RunTransition().Forget();
    }

    private async UniTaskVoid RunTransition()
    {
        await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
        var startingValue = _sourceMaterial.GetFloat(_propertyID);
        _transitionStarted?.Invoke();
        for (var f = 0f; f < 1; f+=Time.deltaTime*_transitionSpeed)
        {
            _sourceMaterial.SetFloat(_propertyID, Mathf.Lerp(startingValue, _targetValue, _transitionCurve.Evaluate(f)));
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
            if (_cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
        _transitionCompleted?.Invoke();
    }
    
    private void ResetMaterial()
    {
        _sourceMaterial.SetFloat(_propertyID, _defaultValue);
    }
    

    private void OnDestroy()
    {
        Application.quitting -= ResetMaterial;
    }
}
