using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class ActiveLaneIndicator : MonoBehaviour, IPoolable
{
    [SerializeField]
    private Renderer _renderer;
    private int _activeFormations = 0;

    private float _currentVisibility;
    private float _targetVisibility;

    private float _updateSpeed = 1f;
    
    private bool _shouldUpdate;
    private bool _shouldRePool;

    private readonly int _visibilityHash = Shader.PropertyToID("_Alpha");
    public int ActiveFormations => _activeFormations;
    public PoolManager MyPoolManager { get; set; }
    public bool IsPooled { get; set; }

    private CancellationToken _cancellationToken;

    private void OnValidate()
    {
        if (_renderer == null)
        {
            _renderer = GetComponentInChildren<Renderer>();
        }
    }

    public void Initialize()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
        _currentVisibility = _renderer.material.GetFloat(_visibilityHash);
        ChangeVisibility().Forget();
    }

    public void SetUp(float rotation, Transform playerCenter)
    {
        transform.position = Vector3.zero;
        transform.RotateAround(playerCenter.position, playerCenter.up, rotation);
        _shouldUpdate = true;
        _targetVisibility = 1;
        _updateSpeed = 15f;
        gameObject.SetActive(true);
    }

    public void AddFormation()
    {
        _activeFormations++;
    }

    public void RemoveFormation()
    {
        if(_activeFormations>0)
        {
            _activeFormations--;
        }
    }

    public void HideAndReturn()
    {
        _targetVisibility = 0;
        _updateSpeed = .25f;
        _shouldUpdate = true;
        _shouldRePool = true;
    }

    public void ReturnToPool()
    {
        _activeFormations = 0;
        gameObject.SetActive(false);
        transform.rotation = quaternion.identity;
        MyPoolManager.ReturnToPool(this);
    }

    private async UniTaskVoid ChangeVisibility()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            if (!_shouldUpdate)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(.05f), cancellationToken: _cancellationToken);
                continue;
            }

            _shouldUpdate = false;
            
            for (var f = 0f; f < 1; f+=Time.deltaTime*_updateSpeed)
            {
                _currentVisibility = Mathf.Lerp(_currentVisibility, _targetVisibility, f);
                _renderer.material.SetFloat(_visibilityHash, _currentVisibility);
                await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
            }

            if (_shouldRePool)
            {
                _shouldRePool = false;
                ReturnToPool();
            }
        }
    }
}
