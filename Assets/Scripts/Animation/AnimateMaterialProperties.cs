using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimateMaterialProperties : MaterialValueLerper
{
    [SerializeField]
    private Renderer[] _targetRenderers;
    [SerializeField]
    private Image[] _targetImages;
    [SerializeField]
    private TextMeshProUGUI[] _targetTexts;
    private Material[] _instancedMaterials;
    [SerializeField]
    private Shader _instancedMatShader;
    [SerializeField]
    private Shader _instancedTextShader;

    [SerializeField]
    private string _enableEffectName;

    [SerializeField]
    private string _disableEffectName;


    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        if (_instancedMaterials == null)
        {            
            _instancedMaterials = new Material[_targetRenderers.Length];
            for (int i = 0; i < _targetRenderers.Length; i++)
            {
                _instancedMaterials[i] = MaterialsManager.Instance.GetInstancedMaterial(_targetRenderers[i]);
                _instancedMaterials[i].shader = _instancedMatShader;
            }
        }
        if(!string.IsNullOrWhiteSpace(_enableEffectName))
        {
            TriggerValueChange(_enableEffectName).Forget();
        }
    }

    private void OnDisable()
    {
        if (_instancedMaterials == null || string.IsNullOrWhiteSpace(_disableEffectName))
        {
            return;
        }

        TriggerValueChange(_disableEffectName).Forget();
    }

    public virtual async UniTaskVoid TriggerValueChange(string effectName)
    {
        foreach (var renderer in _targetRenderers)
        {
            renderer.sharedMaterial = MaterialsManager.Instance.GetInstancedMaterial(renderer);
        }
        foreach (var text in _targetTexts)
        {
            text.materialForRendering.shader = _instancedTextShader;
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

        foreach (var materialValue in _materialValues)
        {
            if (string.Equals(effectName, materialValue.EffectName))
            {
                foreach (var material in _instancedMaterials)
                {
                    materialValue.StartLerpingValue(material, _lerpSpeed, _cancellationTokenSource);
                }
                foreach (var image in _targetImages)
                {
                    materialValue.StartLerpingValue(image.materialForRendering, _lerpSpeed, _cancellationTokenSource);
                }
                foreach (var text in _targetTexts)
                {
                    materialValue.StartLerpingValue(text.materialForRendering, _lerpSpeed, _cancellationTokenSource);
                }
            }
        }
        ResetRenderers();
    }

    private void ResetRenderers()
    {
        foreach (var renderer in _targetRenderers)
        {
            MaterialsManager.Instance.TryGetOriginalMaterial(renderer, out var original);
            renderer.sharedMaterial = original;
        }
        foreach (var text in _targetTexts)
        {
            text.materialForRendering.shader = text.material.shader;
        }
    }
}
