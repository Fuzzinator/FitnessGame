using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAnimatorProperties : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private AnimatorProperty[] _properties;

    private void OnValidate()
    {
        if (_animator == null)
        {
            TryGetComponent(out _animator);
        }
    }

    private void Start()
    {
        foreach (var property in _properties)
        {
            switch (property.ThisType)
            {
                case AnimatorProperty.PropType.Bool:
                    _animator.SetBool(property.PropertyName, property.BoolValue);
                    break;
                case AnimatorProperty.PropType.Int:
                    _animator.SetInteger(property.PropertyName, property.IntValue);
                    break;
                case AnimatorProperty.PropType.Float:
                    _animator.SetFloat(property.PropertyName, property.FloatValue);
                    break;
                case AnimatorProperty.PropType.Trigger:
                    _animator.SetTrigger(property.PropertyName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    [System.Serializable]
    private struct AnimatorProperty
    {
        [SerializeField]
        private string _propertyName;

        [SerializeField]
        private PropType _type;

        [SerializeField]
        private bool _boolValue;

        [SerializeField]
        private int _intValue;

        [SerializeField]
        private float _floatValue;
        
        
        public string PropertyName => _propertyName;
        public PropType ThisType => _type;
        public bool BoolValue => _boolValue;
        public int IntValue => _intValue;
        public float FloatValue => _floatValue;
        
        public enum PropType
        {
            Bool,
            Int,
            Float,
            Trigger
        }
    }
}
