using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTargetHighlightColor : MonoBehaviour
{
    [SerializeField]
    private Renderer _baseTarget;

    [SerializeField]
    private Renderer[] _highlight;

    private bool _initialized;
    
    // Start is called before the first frame update
    public void Initialize()
    {
        var color = _baseTarget.sharedMaterial.color;
        foreach (var rend in _highlight)
        {
            rend.material.color = color;
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
}
