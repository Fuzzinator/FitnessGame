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
    
    [SerializeField]
    private string _scaleValue = "Scale";

    private int _footPositionHash;
    private int _scaleHash;

    private void OnValidate()
    {
        if (_animator == null)
        {
            TryGetComponent(out _animator);
        }
    }

    private void Start()
    {
        _footPositionHash = Animator.StringToHash(_footPositionValue);
        _scaleHash = Animator.StringToHash(_scaleValue);
    }

    public void UpdateFootPlacement(int placement)
    {
        SetAnimatorState(true);
        _animator.SetInteger(_footPositionHash, placement);
    }

    public void ScaleAndUpdatePlacement(int placement)
    {
        SetAnimatorState(true);
        _animator.SetInteger(_footPositionHash, placement);
        _animator.SetTrigger(_scaleHash);
    }
    
    private void SetAnimatorState(bool on)
    {
        _animator.enabled = on;
    }
}
