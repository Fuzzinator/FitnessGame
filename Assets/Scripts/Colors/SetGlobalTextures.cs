using UnityEngine;

[ExecuteAlways]
public class SetGlobalTextures : MonoBehaviour
{
    [SerializeField]
    private TextureSet[] _textureSets;
    [SerializeField]
    private TextureArraySet[] _textureArraySets;
    /*[SerializeField]
    private Texture2DArray _environmentTextures;
    
    [SerializeField]
    private Texture2DArray _environmentNormalTextures;
    
    private const string ENVTEXTURES = "environmentTextures";
    private const string ENVNORMALTEXTURES = "environmentNormalTextures";*/

    private void OnValidate()
    {
        foreach (var set in _textureSets)
        {
            Shader.SetGlobalTexture(set.Name, set.Texture);
        }
        foreach (var set in _textureArraySets)
        {
            Shader.SetGlobalTexture(set.Name, set.Texture);
        }
        /*Shader.SetGlobalTexture(ENVTEXTURES, _environmentTextures);
        Shader.SetGlobalTexture(ENVNORMALTEXTURES, _environmentNormalTextures);*/
    }

    /*private void Start()
    {
        foreach (var set in _textureSets)
        {
            Shader.SetGlobalTexture(set.Name, set.Texture);
        }
        
        foreach (var set in _textureArraySets)
        {
            Shader.SetGlobalTexture(set.Name, set.Texture);
        }
        /*Shader.SetGlobalTexture(ENVTEXTURES, _environmentTextures);
        Shader.SetGlobalTexture(ENVNORMALTEXTURES, _environmentNormalTextures);#1#
    }*/

    private void OnDestroy()
    {
        foreach (var set in _textureSets)
        {
            Shader.SetGlobalTexture(set.Name, null);
        }
        
        foreach (var set in _textureArraySets)
        {
            Shader.SetGlobalTexture(set.Name, null);
        }
        /*Shader.SetGlobalTexture(ENVTEXTURES, null);
        Shader.SetGlobalTexture(ENVNORMALTEXTURES, null);*/
    }
}
