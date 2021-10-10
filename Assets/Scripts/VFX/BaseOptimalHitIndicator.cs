using System;
using System.Collections;
using System.Collections.Generic;
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

    private void Start()
    {
        if (_renderer == null)
        {
            return;
        }

        _propertyHash = Shader.PropertyToID(_propertyName);
        _renderer.material.SetFloat(_propertyHash, 0);
    }

    private async void OnEnable()
    {
        _destroyed = false;
        _renderer.material.SetFloat(_propertyHash, 0);
        await UpdateIndicator();
    }

    public async UniTask UpdateIndicator()
    {
        var hitPoint = _baseTarget.OptimalHitPoint;
        while (enabled && gameObject.activeSelf)
        {
            await UniTask.Yield(this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
            if (_destroyed)
            {
                return;
            }

            _renderer.material.SetFloat(_propertyHash, GetIndicatorStrength());
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
        currentDistance  /= _effectRange;
        return 1 - currentDistance;
    }
}