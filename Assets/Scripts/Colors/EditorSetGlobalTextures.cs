using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class EditorSetGlobalTextures : MonoBehaviour
{
    [SerializeField]
    private TextureSet[] _textureSets;
    [SerializeField]
    private TextureArraySet[] _textureArraySets;
    
    private void Awake()
    {
        SetTextures();
    }

    private void OnValidate()
    {
        SetTextures();
    }

    private void SetTextures()
    {
        foreach (var set in _textureSets)
        {
            Shader.SetGlobalTexture(set.Name, set.Texture);
        }

        foreach (var set in _textureArraySets)
        {
            Shader.SetGlobalTexture(set.Name, set.Texture);
        }
    }
}
