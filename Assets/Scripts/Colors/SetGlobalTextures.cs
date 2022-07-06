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

    private void Start()
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

    [System.Serializable]
    private struct TextureSet
    {
        [SerializeField]
        private string _textureName;

        [SerializeField]
        private Texture2D _texture;

        public string Name => _textureName;
        public Texture2D Texture => _texture;
    }
    
    [System.Serializable]
    private struct TextureArraySet
    {
        [SerializeField]
        private string _textureArrayName;

        [SerializeField]
        private Texture2DArray _textureArray;

        public string Name => _textureArrayName;
        public Texture2DArray Texture => _textureArray;
    }
}
