using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SetRendererMaterial : MonoBehaviour, ITargetInitializer
{
    [SerializeField]
    private Renderer[] _targetRenderers;

    [SerializeField]
    private bool _isTarget;

    [SerializeField]
    private bool _isController;

    [SerializeField]
    private int _textureIndex;

    [SerializeField]
    private int _indexOffset = 1;

    public Renderer[] Renderers => _targetRenderers;

    public void Initialize(HitSideType type, bool superNote)
    {
        SetMaterial(type, superNote);
    }

    public void Initialize(BaseTarget target)
    {
        SetMaterial(target.HitSideType, target.IsSuperNote);
    }

    private void SetMaterial(HitSideType hitSideType, bool superNote)
    {
        foreach (var renderer in _targetRenderers)
        {
            var index = _isTarget ? (int)hitSideType : 0;
            var subIndex = _textureIndex + (_indexOffset * _textureIndex) + (superNote ? _indexOffset : 0);
            renderer.sharedMaterial = MaterialsManager.Instance.GetMaterial(index, subIndex);
        }
    }

}