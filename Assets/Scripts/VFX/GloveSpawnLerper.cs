using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GloveSpawnLerper : MaterialValueLerper
{
    [SerializeField]
    private Renderer[] _gloves = null;

    [SerializeField]
    private Shader _targetShader = null;

    [SerializeField]
    private string _onEnableEventName = null;

    private Material[] _originalMaterials = null;
    private int _activeEffects = 0;


    private void Awake()
    {
        if (_gloves.Length > 0)
        {
            SetUpMaterials();
        }
    }

    private void OnEnable()
    {
        if (!string.IsNullOrWhiteSpace(_onEnableEventName))
        {
            TriggerValueChangeAsync(_onEnableEventName).Forget();
        }
    }

    private void OnDisable()
    {
        TriggerValueChangeAsync("Disable").Forget();
    }

    private void OnDestroy()
    {
        if (_targetMaterial != null)
        {
            Destroy(_targetMaterial);
        }
    }

    public void SetRenderersAndSpawn(Renderer[] renderers)
    {
        _gloves = renderers;
        Awake();
        OnEnable();
    }

    public void SetUpMaterials()
    {
        _originalMaterials = new Material[_gloves.Length];
        for (int i = 0; i < _gloves.Length; i++)
        {
            _originalMaterials[i] = _gloves[i].sharedMaterial;
        }
        _targetMaterial = _gloves[0].material;
        _targetMaterial.shader = _targetShader;
    }

    public void TriggerValueChange(string effectName)
    {
        TriggerValueChangeAsync(effectName).Forget();
    }

    public override async UniTaskVoid TriggerValueChangeAsync(string effectName)
    {
        if (_originalMaterials == null)
        {
            return;
        }
        for (int i = 0; i < _gloves.Length; i++)
        {
            _gloves[i].sharedMaterial = _targetMaterial;
        }

        if (_cancellationTokenSource == null)
        {
            _cancellationToken = gameObject.GetCancellationTokenOnDestroy();
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
        }
        else
        {
            _cancellationTokenSource.Cancel();
        }

        await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
        if (_cancellationTokenSource.IsCancellationRequested && !_cancellationToken.IsCancellationRequested)
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
        }
        _activeEffects = 0;
        foreach (var materialValue in _materialValues)
        {
            if (string.Equals(effectName, materialValue.EffectName))
            {
                _activeEffects++;
                materialValue.completed.AddListener(CheckForMaterialReset);
                materialValue.StartLerpingValue(_targetMaterial, _lerpSpeed, _cancellationTokenSource);
            }
        }
    }

    private void CheckForMaterialReset(MaterialValue value)
    {
        _activeEffects--;
        if (_activeEffects == 0)
        {
            value.completed.RemoveListener(CheckForMaterialReset);
            for (int i = 0; i < _gloves.Length; i++)
            {
                _gloves[i].sharedMaterial = _originalMaterials[i];
            }
        }
    }
}
