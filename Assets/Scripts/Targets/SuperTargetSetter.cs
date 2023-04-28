using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperTargetSetter : MonoBehaviour, ITargetInitializer
{
    [SerializeField]
    private ParticleSystem _particleSystem;
    [SerializeField]
    private BaseTarget _baseTarget;
    [SerializeField]
    private Renderer _sourceRenderer;
    [SerializeField]
    private Renderer _targetRenderer;

    private const string PositionOffset = "_Position_Change";
    private const string RotationOffset = "_DistanceRotation";
    private int _positionOffsetID = int.MinValue;
    private int _rotationOffsetID = int.MinValue;

    private void OnValidate()
    {
        if (_baseTarget == null)
        {
            _baseTarget = GetComponent<BaseTarget>();
        }
    }


    public void Initialize(BaseTarget target)
    {
        _baseTarget = target;
        _particleSystem.gameObject.SetActive(_baseTarget.IsSuperNote);
        if (_particleSystem == null || !_baseTarget.IsSuperNote)
        {
            return;
        }

        var ps = _particleSystem.main;
        ps.startColor = ColorsManager.Instance.GetAppropriateColor(_baseTarget.HitSideType);

        if(_positionOffsetID == int.MinValue)
        {
            _positionOffsetID = Shader.PropertyToID(PositionOffset);
            _rotationOffsetID = Shader.PropertyToID(RotationOffset);
        }
        var positionOffset = _sourceRenderer.sharedMaterial.GetVector(_positionOffsetID);
        var rotationOffset = _sourceRenderer.sharedMaterial.GetFloat(_rotationOffsetID);
        _targetRenderer.sharedMaterial.SetVector(_positionOffsetID, positionOffset);
        _targetRenderer.sharedMaterial.SetFloat(_rotationOffsetID, rotationOffset);
    }
}
