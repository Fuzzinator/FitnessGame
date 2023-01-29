using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRendererColor : MonoBehaviour
{
    [SerializeField]
    private Renderer _sourceRenderer;

    [SerializeField]
    private Renderer _targetRenderer;

    public void Initialize()
    {
        _targetRenderer.sharedMaterial.color = _sourceRenderer.sharedMaterial.color;
    }
}
