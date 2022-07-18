using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class EnvironmentControlManager : MonoBehaviour
{
    public static EnvironmentControlManager Instance;


    [SerializeField]
    private List<AddressableEnvAssetRef> _availableReferences = new List<AddressableEnvAssetRef>();


    private int _targetEnvironmentIndex = 0;

    public EnvironmentAssetContainer ActiveEnvironmentContainer { get; private set; }

    public bool LoadingEnvironmentContainer { get; private set; }

    private const string ADDRESSABLELABEL = "Environment Asset";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        GetBuiltInEnvironments().Forget();
    }

    public void SetTargetEnvironmentIndex(int index)
    {
        _targetEnvironmentIndex = index;
    }

    public void LoadSelection()
    {
        LoadEnvironmentData(_targetEnvironmentIndex);
    }

    private void LoadEnvironmentData(int index)
    {
        LoadEnvironmentDataAsync(index).Forget();
    }

    private async UniTask GetBuiltInEnvironments()
    {
        _availableReferences.Clear();

        await Addressables.LoadAssetsAsync<AddressableEnvAssetRef>(ADDRESSABLELABEL, asset =>
        {
            if (asset == null)
            {
                return;
            }

            _availableReferences.Add(asset);
        });
    }

    private async UniTask LoadEnvironmentDataAsync(int index)
    {
        LoadingEnvironmentContainer = true;
        await Addressables.LoadAssetsAsync<EnvironmentAssetContainer>(_availableReferences[index].AssetReference,
            asset =>
            {
                if (asset == null)
                {
                    Debug.LogError(
                        $"Found null asset when loading environment {_availableReferences[index].EnvironmentName}");
                    LoadingEnvironmentContainer = false;
                    return;
                }

                ActiveEnvironmentContainer = asset;
            });
        ColorsManager.Instance.SetAndUpdateTextureSets(ActiveEnvironmentContainer.TargetTextures,
            ActiveEnvironmentContainer.ObstacleTextures);

        if (ActiveEnvironmentContainer.GlobalTextureSets is {Length: > 0}) //this is equal to is GlobalTextureSets != null && its length>0
        {
            foreach (var set in ActiveEnvironmentContainer.GlobalTextureSets)
            {
                Shader.SetGlobalTexture(set.Name, set.Texture);
            }
        }

        if (ActiveEnvironmentContainer.GlobalTextureArraySets is {Length: > 0})
        {
            foreach (var set in ActiveEnvironmentContainer.GlobalTextureArraySets)
            {
                Shader.SetGlobalTexture(set.Name, set.Texture);
            }
        }

        LoadingEnvironmentContainer = false;
    }

    public List<string> GetNewAvailableEnvironmentsList()
    {
        var references = new List<string>(_availableReferences.Count);
        foreach (var assetRef in _availableReferences)
        {
            references.Add(assetRef.EnvironmentName);
        }

        return references;
    }
}

[Serializable]
public struct TextureArraySet
{
    [SerializeField]
    private string _textureArrayName;

    [SerializeField]
    private Texture2DArray _textureArray;

    public string Name => _textureArrayName;
    public Texture2DArray Texture => _textureArray;
}

[Serializable]
public struct TextureSet
{
    [SerializeField]
    private string _textureName;

    [SerializeField]
    private Texture2D _texture;

    public string Name => _textureName;
    public Texture2D Texture => _texture;
}