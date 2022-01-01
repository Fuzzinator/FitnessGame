using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class BaseOptimalHitIndicator : MonoBehaviour
{
    [SerializeField]
    private BaseTarget _baseTarget;

    [SerializeField]
    private Renderer _renderer;

    [SerializeField]
    private float _effectRange = 2;

    [SerializeField]
    private string _propertyName;

    private int _propertyHash;
    private bool _destroyed = false;
    private CancellationToken _cancellationToken;

    private void Start()
    {
        if (_renderer == null)
        {
            return;
        }

        _propertyHash = Shader.PropertyToID(_propertyName);
        _renderer.material.SetFloat(_propertyHash, 0);
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    private async void OnEnable()
    {
        _destroyed = false;
        _renderer.material.SetFloat(_propertyHash, 0);
        await UpdateIndicator(_cancellationToken);
    }

    public async UniTask UpdateIndicator(CancellationToken token)
    {
        var hitPoint = _baseTarget.OptimalHitPoint;
        var previousStrength = GetIndicatorStrength();
        while (enabled && gameObject.activeSelf)
        {
            try
            {
                await UniTask.DelayFrame(1, cancellationToken: token);
                if (_destroyed)
                {
                    return;
                }

                var newStrength = GetIndicatorStrength();
                if (Math.Abs(previousStrength - newStrength) < .001f)
                {
                    continue;
                }

                _renderer.material.SetFloat(_propertyHash, GetIndicatorStrength());
            }
            catch (Exception e) when (e is OperationCanceledException)
            {
                break;
            }
        }
    }

    private void OnDisable()
    {
        if (_renderer == null)
        {
            return;
        }

        _renderer.material.SetFloat(_propertyHash, 0);
    }

    private void OnDestroy()
    {
        _destroyed = true;

        if (_renderer == null)
        {
            return;
        }

        Destroy(_renderer.material);
    }

    private float GetIndicatorStrength()
    {
        var currentDistance = Vector3.Distance(transform.position, _baseTarget.OptimalHitPoint);
        currentDistance = Mathf.Clamp(currentDistance, 0, _effectRange);
        currentDistance /= _effectRange;
        return 1 - currentDistance;
    }
}