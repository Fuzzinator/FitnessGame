using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MaterialValueLerper : MonoBehaviour
{
    [SerializeField]
    private Material _targetMaterial;

    [SerializeField]
    private float _lerpSpeed = 1f;

    [SerializeReference]
    private List<MaterialValue> _materialValues = new List<MaterialValue>();

    private CancellationTokenSource _cancellationTokenSource;
    private CancellationToken _cancellationToken;


    #region Creating New MaterialValues

    [ContextMenu("Add Color Changer")]
    private void AddColorChanger()
    {
        _materialValues.Add(new ColorValue());
    }

    [ContextMenu("Add Float Changer")]
    private void AddFloatChanger()
    {
        _materialValues.Add(new FloatValue());
    }

    [ContextMenu("Add Vector Changer")]
    private void AddVectorChanger()
    {
        _materialValues.Add(new VectorValue());
    }

    #endregion
    
    public async UniTaskVoid TriggerValueChange(string effectName)
    {
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
        if(_cancellationTokenSource.IsCancellationRequested && !_cancellationToken.IsCancellationRequested)
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
        }
        
        foreach (var materialValue in _materialValues)
        {
            if (string.Equals(effectName, materialValue.EffectName))
            {
                materialValue.StartLerpingValue(_targetMaterial, _lerpSpeed, _cancellationTokenSource);
            }
        }
    }

    #region Material Value Classes

    [Serializable]
    private abstract class MaterialValue
    {
        [SerializeField]
        protected string _materialPropertyName;

        [SerializeField]
        private string _effectName;

        [SerializeField]
        protected float _lerpSpeedModifier = 1f;

        [SerializeField]
        protected AnimationCurve _animationCurve;

        protected int _materialProperty = 0;

        public string EffectName => _effectName;

        protected virtual void GetMaterialSettings(Material mat)
        {
            if (_materialProperty == 0)
            {
                _materialProperty = Shader.PropertyToID(_materialPropertyName);
            }
        }
        
        public virtual void StartLerpingValue(Material material, float speed, CancellationTokenSource cancellationSource,
            Action<Material, float> action = null)
        {
            GetMaterialSettings(material);
            LerpValue(material, speed, cancellationSource, action).Forget();
        }

        private async UniTaskVoid LerpValue(Material material, float speed, CancellationTokenSource cancellationSource,
            Action<Material, float> action)
        {
            while (Math.Abs(Time.deltaTime - 1) < .01f && !cancellationSource.IsCancellationRequested)
            {
                await UniTask.DelayFrame(1, cancellationToken: cancellationSource.Token);
            }
            
            for (var time = 0f; time <= 1; time += Time.smoothDeltaTime * _lerpSpeedModifier * speed)
            {
                var lerpPoint = _animationCurve.Evaluate(time);
                action?.Invoke(material, lerpPoint);
                await UniTask.DelayFrame(1, cancellationToken: cancellationSource.Token);
                if (cancellationSource.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }

    [Serializable]
    private class ColorValue : MaterialValue
    {
        private Color _startValue;
        [SerializeField]
        private Color _endValue;

        protected override void GetMaterialSettings(Material mat)
        {
            base.GetMaterialSettings(mat);
            _startValue = mat.GetColor(_materialProperty);
        }
        public override void StartLerpingValue(Material material, float speed, CancellationTokenSource cancellationSource,
            Action<Material, float> action = null)
        {
            action = (mat, lerpPoint) =>
            {
                mat.SetColor(_materialProperty, Color.Lerp(_startValue, _endValue, lerpPoint));
            };
            base.StartLerpingValue(material, speed, cancellationSource, action);
        }
    }

    [Serializable]
    private class FloatValue : MaterialValue
    {
        private float _startValue;
        [SerializeField]
        private float _endValue;
        protected override void GetMaterialSettings(Material mat)
        {
            base.GetMaterialSettings(mat);
            _startValue = mat.GetFloat(_materialProperty);
        }
        public override void StartLerpingValue(Material material, float speed, CancellationTokenSource cancellationSource,
            Action<Material, float> action = null)
        {
            action = (mat, lerpPoint) =>
            {
                mat.SetFloat(_materialProperty, Mathf.Lerp(_startValue, _endValue, lerpPoint));
            };

            base.StartLerpingValue(material, speed, cancellationSource, action);
        }
    }

    [System.Serializable]
    private class VectorValue : MaterialValue
    {
        private Vector4 _startValue;
        [SerializeField]
        private Vector4 _endValue;
        protected override void GetMaterialSettings(Material mat)
        {
            base.GetMaterialSettings(mat);
            _startValue = mat.GetVector(_materialProperty);
        }
        public override void StartLerpingValue(Material material, float speed, CancellationTokenSource cancellationSource,
            Action<Material, float> action = null)
        {
            action = (mat, lerpPoint) =>
            {
                mat.SetVector(_materialProperty, Vector4.Lerp(_startValue, _endValue, lerpPoint));
            };
            base.StartLerpingValue(material, speed, cancellationSource, action);
        }
    }

    #endregion
}