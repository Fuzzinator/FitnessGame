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

    private int FootPositionHash => _footPositionHash == 0 ? _footPositionHash = Animator.StringToHash(_footPositionValue) : _footPositionHash;
    private int ScaleHash => _scaleHash == 0 ? _scaleHash = Animator.StringToHash(_scaleValue) : _scaleHash;

    private void OnValidate()
    {
        if (_animator == null)
        {
            TryGetComponent(out _animator);
        }
    }

    public void UpdateFootPlacement(int placement)
    {
        SetObjectsState(true);
        _animator.SetInteger(FootPositionHash, placement);
    }

    public void ScaleAndUpdatePlacement(int placement)
    {
        SetObjectsState(true);
        _animator.SetInteger(FootPositionHash, placement);
        _animator.SetTrigger(ScaleHash);
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
