using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
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

    [SerializeField]
    private List<EnvGlovesRef> _availableGloveReferences = new List<EnvGlovesRef>();
    [SerializeField]
    private List<EnvTargetsRef> _availableTargetReferences = new List<EnvTargetsRef>();
    [SerializeField]
    private List<EnvObstaclesRef> _availableObstacleReferences = new List<EnvObstaclesRef>();
    [SerializeField]
    private Material _customEnvironmentSkyboxMat;
    private Texture2D _activeCustomSkybox;
    public Texture2D Skybox => _activeCustomSkybox;
    private string _customSkyboxPath;

    private List<Environment> _availableEnvironments = new List<Environment>();

    public UnityEvent availableReferencesUpdated = new UnityEvent();
    public UnityEvent availableGloveRefsUpdated = new UnityEvent();
    public UnityEvent availableTargetRefsUpdated = new UnityEvent();
    public UnityEvent availableObstacleRefsUpdated = new UnityEvent();
    public UnityEvent<int> targetEnvironmentIndexChanged = new UnityEvent<int>();

    private int _targetEnvironmentIndex = 0;

    public EnvironmentAssetContainer ActiveEnvironmentContainer { get; private set; }

    public bool LoadingEnvironmentContainer { get; private set; }

    private List<AsyncOperationHandle> _assetHandles = new List<AsyncOperationHandle>();

    private AddressableEnvAssetRef _customEnvironment;
    private System.Threading.CancellationToken _cancellationToken;

    private const string ADDRESSABLELABEL = "Environment Asset";
    private const string CustomSkyboxAlbedo = "_SkyboxColor";
    private const string CustomSkyboxExposure = "_SkyboxExposure";

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
        _cancellationToken = this.GetCancellationTokenOnDestroy();
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
        _availableEnvironments.Sort((x, y) => x.Name.CompareTo(y.Name));
        availableReferencesUpdated.Invoke();
    }

    public void SetTargetEnvironmentIndex(int index)
    {
        _targetEnvironmentIndex = index;
        targetEnvironmentIndexChanged.Invoke(index);
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
        for (var i = 0; i < _availableEnvironments.Count; i++)
        {
            var option = _availableEnvironments[i];
            if (string.Equals(option.Name, sceneName, StringComparison.InvariantCultureIgnoreCase))
            {
                index = i;
                break;
            }
        }

        return index;
    }

    public string GetTargetEnvName()
    {
        return _availableEnvironments[_targetEnvironmentIndex].Name;
    }

    private void LoadEnvironmentData(int index)
    {
        var targetEnv = _availableEnvironments[index];
        if (targetEnv.IsCustom)
        {
            LoadCustomEnvironmentDataAsync(targetEnv.CustomPath).Forget();
        }
        else
        {
            LoadBuiltInEnvironmentDataAsync(targetEnv.AssetRef.AssetReference).Forget();
        }
    }

    private async UniTask<List<string>> GetCustomEnvironments()
    {
        _availableCustomEnvironments.Clear();

        var environments = CustomEnvironmentsController.RefreshAvailableCustomEnvironments();
        foreach (var environment in environments)
        {
            var customEnv = await CustomEnvironmentsController.LoadCustomEnvironment(environment);
            if (customEnv != null && customEnv.isValid)
            {
                var env = new Environment(environment, customEnv);
                _availableEnvironments.Add(env);
            }
        }
        return environments;
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
                    _availableEnvironments.Add(new Environment(asset));
                    break;
                case TargetPlatform.CustomEnvironment:
                    _customEnvironment = asset;
                    break;
                default:
                    break;
            }

        });
        _assetHandles.Add(results);
        await results;
    }

    private async UniTask GetBuiltInEnvAssetRefs()
    {

    }

    public bool TryGetEnvRefAtIndex(int index, out Environment environment)
    {
        if(_availableEnvironments.Count>index)
        {
            environment = _availableEnvironments[index];
            return true;
        }
        environment = new Environment();
        return false;
    }

    private async UniTaskVoid LoadCustomEnvironmentDataAsync(string environmentPath)
    {
        LoadingEnvironmentContainer = true;
        await LoadEnvironmentDataAsync(_customEnvironment.AssetReference);
        var environment = await CustomEnvironmentsController.LoadCustomEnvironment(environmentPath);
        if (!string.Equals(_customSkyboxPath, environment.SkyboxPath))
        {
            _customSkyboxPath = environment.SkyboxPath;
            _activeCustomSkybox = await CustomEnvironmentsController.LoadEnvironmentTexture(environment.SkyboxPath, _cancellationToken);
        }
        Shader.SetGlobalTexture(CustomSkyboxAlbedo, _activeCustomSkybox);
        Shader.SetGlobalFloat(CustomSkyboxExposure, environment.SkyboxBrightness);

        LoadingEnvironmentContainer = false;
    }

    private async UniTaskVoid LoadBuiltInEnvironmentDataAsync(AssetReference reference)
    {
        LoadingEnvironmentContainer = true;
        await LoadEnvironmentDataAsync(reference);
        LoadingEnvironmentContainer = false;
    }

    private async UniTask LoadEnvironmentDataAsync(AssetReference reference)
    {
        var results = Addressables.LoadAssetsAsync<EnvironmentAssetContainer>(reference,
            asset =>
            {
                if (asset == null)
                {
                    Debug.LogError(
                        $"Found null asset when loading environment {reference}");
                    LoadingEnvironmentContainer = false;
                    return;
                }

                ActiveEnvironmentContainer = asset;
            });
        _assetHandles.Add(results);
        await results;

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
    }

    public void UpdateObstacleTargetTextures()
    {
        ColorsManager.Instance.SetAndUpdateTextureSets(ActiveEnvironmentContainer.TargetTextures,
            ActiveEnvironmentContainer.ObstacleTextures);
    }

    public List<string> GetNewAvailableEnvironmentsList()
    {
        var references = new List<string>(_availableEnvironments.Count);
        foreach (var environment in _availableEnvironments)
        {
            references.Add(environment.Name);
        }

        return references;
    }

    public void AddCustomEnvironment(CustomEnvironment customEnvironment, string path)
    {
        _availableCustomEnvironments.Add(customEnvironment);

        _availableEnvironments.Add(new Environment(path, customEnvironment));
        _availableEnvironments.Sort((x, y) => x.Name.CompareTo(y.Name));

        availableReferencesUpdated.Invoke();
    }

    public void RemoveCustomEnvironment(CustomEnvironment customEnvironment, string path)
    {
        _availableCustomEnvironments.Remove(customEnvironment);
        var index = _availableEnvironments.FindIndex((x) => string.Equals(x.Name, customEnvironment.EnvironmentName, StringComparison.InvariantCultureIgnoreCase));
        if (index >= 0)
        {
            _availableEnvironments.RemoveAt(index);

            availableReferencesUpdated.Invoke();
        }
    }
    public void RemoveCustomEnvironment(string environmentName, string path)
    {
        var custonEnvironIndex = _availableCustomEnvironments.FindIndex((x) => x.EnvironmentName == environmentName);
        if (custonEnvironIndex >= 0)
        {
            _availableCustomEnvironments.RemoveAt(custonEnvironIndex);
        }

        var index = _availableEnvironments.FindIndex((x) => string.Equals(x.Name, environmentName, StringComparison.InvariantCultureIgnoreCase));
        if (index >= 0)
        {
            _availableEnvironments.RemoveAt(index);

            availableReferencesUpdated.Invoke();
        }
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