using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RotationIndicator : MonoBehaviour
{
    [SerializeField]
    private Transform _targetRotation;


    [SerializeField]
    private MeshRenderer _renderer;
    private Material _material;
    
    [SerializeField]
    private string _shaderPropName;

    private int _shaderPropID;
    private float _shaderValue = 0;
    private CancellationToken _token;
    
    
    void Start()
    {
        _token = this.GetCancellationTokenOnDestroy();
        _shaderPropID = Shader.PropertyToID(_shaderPropName);
        _material = _renderer.material;
        MonitorHeadRotation().Forget();
    }

    private void OnDestroy()
    {
        Destroy(_material);
    }

    private async UniTaskVoid MonitorHeadRotation()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(.1f), cancellationToken: _token);
        while (!_token.IsCancellationRequested)
        {
            var dot = Vector3.Dot(_targetRotation.forward, Head.Instance.transform.forward);
            if (_shaderValue < 1 && dot < .85f)
            {
                for (var f = _shaderValue; f < 1; f+=Time.deltaTime)
                {
                    _shaderValue = f;
                    _material.SetFloat(_shaderPropID, _shaderValue);
                    await UniTask.DelayFrame(1, cancellationToken: _token);
                    
                    if (_token.IsCancellationRequested)
                    {
                        return;
                    }
                    var newDot = Vector3.Dot(_targetRotation.forward, Head.Instance.transform.forward);
                    if (newDot >= .85f)
                    {
                        break;
                    }
                }
            }
            else if (_shaderValue > 0 && dot > .85f)
            {
                for (var f = _shaderValue; f > 0; f-=Time.deltaTime)
                {
                    _shaderValue = f;
                    _material.SetFloat(_shaderPropID, _shaderValue);
                    await UniTask.DelayFrame(1, cancellationToken: _token);
                    if (_token.IsCancellationRequested)
                    {
                        return;
                    }
                    var newDot = Vector3.Dot(_targetRotation.forward, Head.Instance.transform.forward);
                    if (newDot < .85f)
                    {
                        break;
                    }
                }
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(.25f), cancellationToken: _token);
        }
    }
}
