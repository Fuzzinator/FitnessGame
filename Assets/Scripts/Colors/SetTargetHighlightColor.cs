using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTargetHighlightColor : MonoBehaviour, IInitializer
{
    [SerializeField]
    private Renderer _baseTarget;

    [SerializeField]
    private Renderer[] _highlight;

    private bool _initialized;

    private readonly int _positionChange = Shader.PropertyToID("_Position_Change");
    
    // Start is called before the first frame update
    public void Initialize(BaseTarget target)
    {
        var color = _baseTarget.sharedMaterial.color;
        //var offset = _baseTarget.sharedMaterial.GetVector(_positionChange);
        foreach (var rend in _highlight)
        {
            rend.material.color = color;
            //rend.material.SetVector(_positionChange, offset);
        }

        _initialized = true;
    }

    private void OnDestroy()
    {
        foreach (var rend in _highlight)
        {
            Destroy(rend.material);
        }
    }

    public void Initialize(BaseObstacle obstacle) { }
}
