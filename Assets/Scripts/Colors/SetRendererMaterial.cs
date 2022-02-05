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

    public void Initialize(HitSideType type)
    {
        SetMaterial(type);
    }
    
    public void SetMaterial(HitSideType hitSideType)
    {
        var currentMaterial = _targetRenderer.sharedMaterial;
        
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