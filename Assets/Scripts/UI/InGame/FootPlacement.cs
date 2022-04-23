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

    [SerializeField]
    private GameObject[] _gameObjects;
    
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
        SetObjectsState(true);
        _animator.SetInteger(_footPositionHash, placement);
    }

    public void ScaleAndUpdatePlacement(int placement)
    {
        SetObjectsState(true);
        _animator.SetInteger(_footPositionHash, placement);
        _animator.SetTrigger(_scaleHash);
    }

    public void DisableAnimator()
    {
        SetObjectsState(false);
    }
    private void SetObjectsState(bool on)
    {
        foreach (var go in _gameObjects)
        {
            go.SetActive(on);
        }
        //_animator.enabled = on;
    }
}
