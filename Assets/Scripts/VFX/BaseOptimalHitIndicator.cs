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
    private string _propertyName;

    private int _propertyHash;

    private void Start()
    {
        if (_renderer == null)
        {
            return;
        }

        _propertyHash = Shader.PropertyToID(_propertyName);
        _renderer.material.SetFloat(_propertyHash, 0);
        
        OnEnable();
    }

    private void OnEnable()
    {
        if (_propertyHash == 0)
        {
            return;
        }
        _renderer.material.SetVector(_propertyHash, _baseTarget.OptimalHitPoint);
    }

    private void OnDestroy()
    {
        if (_renderer == null || _renderer.material == null)
        {
            return;
        }

        Destroy(_renderer.material);
    }
}