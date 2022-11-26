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

    private void Start()
    {
        _cancellationToken = gameObject.GetCancellationTokenOnDestroy();
    }

    public void TriggerValueChange(string effectName)
    {
        foreach (var materialValue in _materialValues)
        {
            if (string.Equals(effectName, materialValue.EffectName))
            {
                materialValue.StartLerpingValue(_targetMaterial, _lerpSpeed, _cancellationToken);
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

        public virtual void StartLerpingValue(Material material, float speed, CancellationToken cancellationToken,
            Action<Material, float> action = null)
        {
            if (_materialProperty == 0)
            {
                _materialProperty = Shader.PropertyToID(_materialPropertyName);
            }
            LerpValue(material, speed, cancellationToken, action).Forget();
        }

        private async UniTaskVoid LerpValue(Material material, float speed, CancellationToken cancellationToken,
            Action<Material, float> action)
        {
            while (Math.Abs(Time.deltaTime - 1) < .01f && !cancellationToken.IsCancellationRequested)
            {
                await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
            }
            
            for (var time = 0f; time <= 1; time += Time.smoothDeltaTime * _lerpSpeedModifier * speed)
            {
                var lerpPoint = _animationCurve.Evaluate(time);
                action?.Invoke(material, lerpPoint);
                await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }

    [Serializable]
    private class ColorValue : MaterialValue
    {
        [SerializeField]
        private Color _startValue;

        [SerializeField]
        private Color _endValue;

        public override void StartLerpingValue(Material material, float speed, CancellationToken cancellationToken,
            Action<Material, float> action = null)
        {
            action = (mat, lerpPoint) =>
            {
                mat.SetColor(_materialProperty, Color.Lerp(_startValue, _endValue, lerpPoint));
            };
            base.StartLerpingValue(material, speed, cancellationToken, action);
        }
    }

    [Serializable]
    private class FloatValue : MaterialValue
    {
        [SerializeField]
        private float _startValue;

        [SerializeField]
        private float _endValue;

        public override void StartLerpingValue(Material material, float speed, CancellationToken cancellationToken,
            Action<Material, float> action = null)
        {
            action = (mat, lerpPoint) =>
            {
                mat.SetFloat(_materialProperty, Mathf.Lerp(_startValue, _endValue, lerpPoint));
            };

            base.StartLerpingValue(material, speed, cancellationToken, action);
        }
    }

    [System.Serializable]
    private class VectorValue : MaterialValue
    {
        [SerializeField]
        private Vector4 _startValue;

        [SerializeField]
        private Vector4 _endValue;

        public override void StartLerpingValue(Material material, float speed, CancellationToken cancellationToken,
            Action<Material, float> action = null)
        {
            action = (mat, lerpPoint) =>
            {
                mat.SetVector(_materialProperty, Vector4.Lerp(_startValue, _endValue, lerpPoint));
            };
            base.StartLerpingValue(material, speed, cancellationToken, action);
        }
    }

    #endregion
}