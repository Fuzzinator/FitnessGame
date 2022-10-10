using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRendererMaterial : MonoBehaviour
{
    [SerializeField]
    private Renderer[] _targetRenderers;

    [SerializeField]
    private bool _isTarget;

    [SerializeField]
    private bool _isController;

    [SerializeField]
    private int _textureIndex;

    public Renderer[] Renderers => _targetRenderers;

    public void Initialize(HitSideType type)
    {
        SetMaterial(type);
    }

    private void SetMaterial(HitSideType hitSideType)
    {
        foreach (var renderer in _targetRenderers)
        {
            var index = _isTarget ? (int) hitSideType : 0;
            var subIndex = _textureIndex;
            renderer.sharedMaterial = MaterialsManager.Instance.GetMaterial(index, subIndex);
        }
    }
    
}