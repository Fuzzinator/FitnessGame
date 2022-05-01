using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRendererMaterial : MonoBehaviour
{
    [SerializeField]
    private Renderer _targetRenderer;

    [SerializeField]
    private bool _isTarget;

    [SerializeField]
    private bool _isController;

    [SerializeField]
    private int _textureIndex;

    [SerializeField]
    private bool _instanceMaterials = false;

    private Material _leftMat;
    private Material _rightMat;

    public void Initialize(HitSideType type)
    {
        if (_instanceMaterials)
        {
            _instanceMaterials = false;
            InstanceMaterials();
        }

        SetMaterial(type);
    }

    private void InstanceMaterials()
    {
        var hex = Shader.PropertyToID("_TextureIndex");
        
        var currentMaterial = _targetRenderer.sharedMaterial;
        _leftMat = new Material(_isTarget ? MaterialsManager.Instance.LeftTarget :
            _isController ? MaterialsManager.Instance.LeftController : currentMaterial);
        _rightMat = new Material(_isTarget ? MaterialsManager.Instance.RightTarget :
            _isController ? MaterialsManager.Instance.RightController : currentMaterial);
        
        _leftMat.SetInt(hex, _textureIndex);
        _rightMat.SetInt(hex, _textureIndex);
    }

    private void SetMaterial(HitSideType hitSideType)
    {
        var currentMaterial = _targetRenderer.sharedMaterial;

        if (_instanceMaterials)
        {
            var material = hitSideType switch
            {
                HitSideType.Block => MaterialsManager.Instance.CenterTarget,
                HitSideType.Left => _leftMat,
                HitSideType.Right => _leftMat,
            };
            
            _targetRenderer.sharedMaterial = material;
        }
        else
        {
            var material = hitSideType switch
            {
                HitSideType.Block => MaterialsManager.Instance.CenterTarget,
                HitSideType.Left => _isTarget ? MaterialsManager.Instance.LeftTarget :
                    _isController ? MaterialsManager.Instance.LeftController : currentMaterial,
                HitSideType.Right => _isTarget ? MaterialsManager.Instance.RightTarget :
                    _isController ? MaterialsManager.Instance.RightController : currentMaterial,
                _ => currentMaterial
            };

            _targetRenderer.sharedMaterial = material;
        }
    }

    private void OnDestroy()
    {
        if (_instanceMaterials)
        {
            Destroy(_leftMat);
            Destroy(_rightMat);
        }
    }
}