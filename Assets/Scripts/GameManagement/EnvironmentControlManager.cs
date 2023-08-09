using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnvironmentControlManager : MonoBehaviour
{
    public static EnvironmentControlManager Instance;


    [SerializeField]
    private List<AddressableEnvAssetRef> _availableReferences = new List<AddressableEnvAssetRef>();
    [SerializeField]
    private List<CustomEnvironment> _availableCustomEnvironments = new List<CustomEnvironment>();

    public UnityEvent availableReferencesUpdated = new UnityEvent();

    private int _targetEnvironmentIndex = 0;

    public EnvironmentAssetContainer ActiveEnvironmentContainer { get; private set; }

    public bool LoadingEnvironmentContainer { get; private set; }

    private List<AsyncOperationHandle> _assetHandles = new List<AsyncOperationHandle>();

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
        UpdateEnvironments().Forget();
    }

    private void OnDestroy()
    {
        foreach (var assetHandle in _assetHandles)
        {
            Addressables.Release(assetHandle);
        }
        _assetHandles.Clear();
    }

    private async UniTaskVoid UpdateEnvironments()
    {
        await GetCustomEnvironments();
        await GetBuiltInEnvironments();

        _availableReferences.Sort((x, y) => x.EnvironmentName.CompareTo(y.EnvironmentName));
    }

    public void SetTargetEnvironmentIndex(int index)
    {
        _targetEnvironmentIndex = index;
    }

    public void LoadSelection()
    {
        LoadEnvironmentData(_targetEnvironmentIndex);

        availableReferencesUpdated?.Invoke();
    }

    public void LoadFromString(string sceneName)
    {
        var index = GetSceneIndexFromString(sceneName);
        SetTargetEnvironmentIndex(index);
        LoadSelection();
    }

    private int GetSceneIndexFromString(string sceneName)
    {
        var index = 0;
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return index;
        }
        for (var i = 0; i < _availableReferences.Count; i++)
        {
            var option = _availableReferences[i];
            if (string.Equals(option.EnvironmentName, sceneName, StringComparison.InvariantCultureIgnoreCase))
            {
                index = i;
                break;
            }
        }

        return index;
    }

    public string GetTargetEnvName()
    {
        return _availableReferences[_targetEnvironmentIndex].EnvironmentName;
    }

    private void LoadEnvironmentData(int index)
    {
        LoadEnvironmentDataAsync(index).Forget();
    }

    private async UniTask GetCustomEnvironments()
    {
        _availableCustomEnvironments.Clear();

        var results = CustomEnvironmentsController.RefreshAvailableCustomEnvironments();
    }

    private async UniTask GetBuiltInEnvironments()
    {
        _availableReferences.Clear();

        var results = Addressables.LoadAssetsAsync<AddressableEnvAssetRef>(ADDRESSABLELABEL, asset =>
        {
            if (asset == null)
            {
                return;
            }

            switch (asset.TargetPlatform)
            {
                case TargetPlatform.All:
#if UNITY_ANDROID
                case TargetPlatform.Android:
#elif UNITY_STANDALONE_WIN
                case TargetPlatform.PCVR:
#endif
                    _availableReferences.Add(asset);
                    break;
                default:
                    break;
            }

        });
        _assetHandles.Add(results);
        await results;
    }

    

    private async UniTask LoadEnvironmentDataAsync(int index)
    {
        LoadingEnvironmentContainer = true;
        var results = Addressables.LoadAssetsAsync<EnvironmentAssetContainer>(_availableReferences[index].AssetReference,
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
        _assetHandles.Add(results);
        await results;
        //ColorsManager.Instance.SetAndUpdateTextureSets(ActiveEnvironmentContainer.TargetTextures,
        //    ActiveEnvironmentContainer.ObstacleTextures);

        if (ActiveEnvironmentContainer.GlobalTextureSets is { Length: > 0 }) //this is equal to is GlobalTextureSets != null && its length>0
        {
            foreach (var set in ActiveEnvironmentContainer.GlobalTextureSets)
            {
                Shader.SetGlobalTexture(set.Name, set.Texture);
            }
        }

        if (ActiveEnvironmentContainer.GlobalTextureArraySets is { Length: > 0 })
        {
            foreach (var set in ActiveEnvironmentContainer.GlobalTextureArraySets)
            {
                Shader.SetGlobalTexture(set.Name, set.Texture);
            }
        }

        MaterialsManager.Instance.SetUpMaterials(ActiveEnvironmentContainer.TargetMaterial, ActiveEnvironmentContainer.ObstacleMaterial, ActiveEnvironmentContainer.SuperTargetMaterial);

        LoadingEnvironmentContainer = false;
    }

    public void UpdateObstacleTargetTextures()
    {
        ColorsManager.Instance.SetAndUpdateTextureSets(ActiveEnvironmentContainer.TargetTextures,
            ActiveEnvironmentContainer.ObstacleTextures);
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