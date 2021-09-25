using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootPlacement : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private string _footPositionValue = "Target Stance";

    private int _propertyHash;

    private void OnValidate()
    {
        if (_animator == null)
        {
            TryGetComponent(out _animator);
        }
    }

    private void Awake()
    {
        _propertyHash = Animator.StringToHash(_footPositionValue);
    }

    public void UpdateFootPlacement(int placement)
    {
        _animator.SetInteger(_propertyHash, placement);
    }
}
