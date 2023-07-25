using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetParticleSystemColor : MonoBehaviour, IInitializer
{
    [SerializeField]
    private Renderer _sourceRenderer;

    [SerializeField]
    private ParticleSystem _targetSystem;

    public void Initialize(BaseTarget target)
    {
        SetColor();
    }


    public void Initialize(BaseObstacle obstacle)
    {
        SetColor();
    }

    private void SetColor()
    {
        var system = _targetSystem.main;
        system.startColor = _sourceRenderer.sharedMaterial.color;
    }
}
