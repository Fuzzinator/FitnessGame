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
    private List<EnvAssetRef> _availableGloveReferences = new List<EnvAssetRef>();
    [SerializeField]
    private List<EnvAssetRef> _availableTargetReferences = new List<EnvAssetRef>();
    [SerializeField]
    private List<EnvAssetRef> _availableObstacleReferences = new List<EnvAssetRef>();

    public int AvailableGloveCount => _availableGloveReferences.Count;
    public int AvailableTargetCount => _availableTargetReferences.Count;
    public int AvailableObstacleCount => _availableObstacleReferences.Count;



    [SerializeField]
    private Material _customEnvironmentSkyboxMat;
    private Texture2D _activeCustomSkybox;
    public Texture2D Skybox => _activeCustomSkybox;
    private string _customSkyboxPath;

    private List<Environment> _availableEnvironments = new List<Environment>();

    public UnityEvent availableReferencesUpdated = new UnityEvent();
    public UnityEvent<int> targetEnvironmentIndexChanged = new UnityEvent<int>();

    private int _targetEnvironmentIndex = 0;


    public EnvAssetReference GlovesOverride { get; private set; }
    public EnvAssetReference TargetsOverride { get; private set; }
    public EnvAssetReference ObstaclesOverride { get; private set; }

    public EnvironmentAssetContainer ActiveEnvironmentContainer { get; private set; }

    public bool LoadingEnvironmentContainer { get; private set; }

    private List<AsyncOperationHandle> _assetHandles = new List<AsyncOperationHandle>();

    private AddressableEnvAssetRef _customEnvironment;
    private CancellationToken _cancellationToken;

    private const string BuiltInEnvLabel = "Environment Asset";
    private const string EnvGlovesLabel = "Environment Gloves";
    private const string EnvTargetsLabel = "Environment Targets";
    private const string EnvObstaclesLabel = "Environment Obstacles";
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
        await GetBuiltInEnvAssetRefs();

        _availableReferences.Sort((x, y) => x.EnvironmentName.CompareTo(y.EnvironmentName));
        _availableEnvironments.Sort((x, y) => x.Name.CompareTo(y.Name));
        _availableGloveReferences.Sort((x, y) => x.AssetName.CompareTo(y.AssetName));
        _availableTargetReferences.Sort((x, y) => x.AssetName.CompareTo(y.AssetName));
        _availableObstacleReferences.Sort((x, y) => x.AssetName.CompareTo(y.AssetName));
        availableReferencesUpdated.Invoke();
    }

    public void SetTargetEnvironmentIndex(int index)
    {
        _targetEnvironmentIndex = index;
        targetEnvironmentIndexChanged.Invoke(index);
    }

    public EnvAssetReference SetGloveOverride(int index)
    {
        if (index < 0 || index >= _availableGloveReferences.Count)
        {
            GlovesOverride = null;
        }
        else
        {
            GlovesOverride = new EnvAssetReference(_availableGloveReferences[index]);
        }

        return GlovesOverride;
    }

    public EnvAssetReference SetTargetOverride(int index)
    {
        if (index < 0 || index >= _availableTargetReferences.Count)
        {
            TargetsOverride = null;
        }
        else
        {
            TargetsOverride = new EnvAssetReference(_availableTargetReferences[index]);
        }

        return TargetsOverride;
    }

    public EnvAssetReference SetObstacleOverride(int index)
    {
        if (index < 0 || index >= _availableObstacleReferences.Count)
        {
            ObstaclesOverride = null;
        }
        else
        {
            ObstaclesOverride = new EnvAssetReference(_availableObstacleReferences[index]);
        }

        return ObstaclesOverride;
    }

    public void SetGloveOverride(EnvAssetReference gloveOverride)
    {
        GlovesOverride = gloveOverride;
    }

    public void SetTargetOverride(EnvAssetReference targetOverride)
    {
        TargetsOverride = targetOverride;
    }

    public void SetObstaclesOverride(EnvAssetReference obstacleOverride)
    {
        ObstaclesOverride = obstacleOverride;
    }

    public void LoadSelection()
    {
        LoadEnvironmentData(_targetEnvironmentIndex);

        availableReferencesUpdated?.Invoke();
    }

    public void LoadFromPlaylist(Playlist playlist)
    {
        var index = GetSceneIndexFromString(playlist.TargetEnvName);
        SetTargetEnvironmentIndex(index);
        GlovesOverride = playlist.Gloves;
        TargetsOverride = playlist.Targets;
        ObstaclesOverride = playlist.Obstacles;

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

    public EnvAssetReference GetGloveAtIndex(int index)
    {
        if (index >= _availableGloveReferences.Count)
        {
            return null;
        }
        return new EnvAssetReference(_availableGloveReferences[index]);
    }

    public EnvAssetReference GetTargetAtIndex(int index)
    {
        if (index >= _availableTargetReferences.Count)
        {
            return null;
        }
        return new EnvAssetReference(_availableTargetReferences[index]);
    }

    public EnvAssetReference GetObstacleAtIndex(int index)
    {
        if (index >= _availableObstacleReferences.Count)
        {
            return null;
        }
        return new EnvAssetReference(_availableObstacleReferences[index]);
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
            LoadCustomEnvironmentDataAsync(targetEnv).Forget();
        }
        else
        {
            LoadBuiltInEnvironmentDataAsync(targetEnv.AssetRef).Forget();
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

        var results = Addressables.LoadAssetsAsync<AddressableEnvAssetRef>(BuiltInEnvLabel, asset =>
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
        _availableGloveReferences.Clear();
        _availableTargetReferences.Clear();
        _availableObstacleReferences.Clear();

        var glovesHandle = Addressables.LoadAssetsAsync<EnvAssetRef>(EnvGlovesLabel, asset =>
        {
            if (asset == null)
            {
                return;
            }
            _availableGloveReferences.Add(asset);
        });
        _assetHandles.Add(glovesHandle);

        var targetsHandle = Addressables.LoadAssetsAsync<EnvAssetRef>(EnvTargetsLabel, asset =>
        {
            if (asset == null)
            {
                return;
            }
            _availableTargetReferences.Add(asset);
        });
        _assetHandles.Add(targetsHandle);

        var obstaclesHandle = Addressables.LoadAssetsAsync<EnvAssetRef>(EnvObstaclesLabel, asset =>
        {
            if (asset == null)
            {
                return;
            }
            _availableObstacleReferences.Add(asset);
        });
        _assetHandles.Add(obstaclesHandle);

        await targetsHandle;
        await glovesHandle;
        await obstaclesHandle;
    }

    public bool TryGetEnvRefAtIndex(int index, out Environment environment)
    {
        if (_availableEnvironments.Count > index)
        {
            environment = _availableEnvironments[index];
            return true;
        }
        environment = new Environment();
        return false;
    }

    public bool TryGetEnvRefByName(string envName, out Environment environment)
    {
        var index = _availableEnvironments.FindIndex(e => e.Name == envName);
        if (index == -1)
        {
            environment = new Environment();
            return false;
        }
        else
        {
            environment = _availableEnvironments[index];
            return true;
        }
    }

    private async UniTaskVoid LoadCustomEnvironmentDataAsync(Environment environment)
    {
        LoadingEnvironmentContainer = true;

        var useEnvGloves = environment.Gloves.IsNotNull();
        var useEnvTargets = environment.Targets.IsNotNull();
        var useEnvObstacles = environment.Obstacles.IsNotNull();

        var assets = new EnvAssets(environment.Name, _customEnvironment.Scene,
                                   useEnvGloves ? environment.Gloves : new(_customEnvironment.Gloves),
                                   useEnvTargets ? environment.Targets : new(_customEnvironment.Targets),
                                   useEnvObstacles ? environment.Obstacles : new(_customEnvironment.Obstacles));

        await TryLoadEnvironmentData(assets);
        var env = await CustomEnvironmentsController.LoadCustomEnvironment(environment.CustomPath);
        if (!string.Equals(_customSkyboxPath, env.SkyboxPath))
        {
            _customSkyboxPath = env.SkyboxPath;
            _activeCustomSkybox = await CustomEnvironmentsController.LoadEnvironmentTexture(env.SkyboxPath, _cancellationToken);
        }
        Shader.SetGlobalTexture(CustomSkyboxAlbedo, _activeCustomSkybox);
        Shader.SetGlobalFloat(CustomSkyboxExposure, env.SkyboxBrightness);

        SetTexturesAndMaterials();
        LoadingEnvironmentContainer = false;
    }

    private async UniTaskVoid LoadBuiltInEnvironmentDataAsync(AddressableEnvAssetRef reference)
    {
        LoadingEnvironmentContainer = true;
        var envDataLoaded = await TryLoadEnvironmentData(new EnvAssets(reference));
        if (!envDataLoaded)
        {
            return;
        }
        SetTexturesAndMaterials();
        LoadingEnvironmentContainer = false;
    }

    private async UniTask<bool> TryLoadEnvironmentData(EnvAssets assets)
    {
        var scene = await Addressables.LoadAssetAsync<EnvSceneRef>(assets.Scene.AssetPath);
        if (scene == null)
        {
            Debug.LogError($"Found null asset when loading environment scene {assets.Scene.AssetName}.");
            return false;
        }

        var glovesRef = (GlovesOverride.IsNotNull() ? GlovesOverride : assets.Gloves);
        var gloves = await Addressables.LoadAssetAsync<EnvGlovesRef>(glovesRef.AssetPath);
        if (gloves == null)
        {
            Debug.LogError($"Found null asset when loading environment scene {glovesRef.AssetName}.");
            return false;
        }

        var targetsRef = TargetsOverride.IsNotNull() ? TargetsOverride : assets.Targets;
        var targets = await Addressables.LoadAssetAsync<EnvTargetsRef>(targetsRef.AssetPath);
        if (targets == null)
        {
            Debug.LogError($"Found null asset when loading environment scene {targetsRef.AssetName}.");
            return false;
        }

        var obstaclesRef = ObstaclesOverride.IsNotNull() ? ObstaclesOverride : assets.Obstacles;
        var obstacles = await Addressables.LoadAssetAsync<EnvObstaclesRef>(obstaclesRef.AssetPath);
        if (obstacles == null)
        {
            Debug.LogError($"Found null asset when loading environment scene {obstaclesRef.AssetName}.");
            return false;
        }


        ActiveEnvironmentContainer = new EnvironmentAssetContainer(assets.EnvironmentName, scene, gloves, targets, obstacles);

        return true;
    }

    private void SetTexturesAndMaterials()
    {
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

        MaterialsManager.Instance.SetUpMaterials(ActiveEnvironmentContainer.TargetMaterial,
            ActiveEnvironmentContainer.ObstacleMaterial, ActiveEnvironmentContainer.SuperTargetMaterial);
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

    public void UpdateEnvironment(string originalName, CustomEnvironment customEnv)
    {
        var envIndex = _availableEnvironments.FindIndex((i) => i.IsCustom && string.Equals(i.Name, originalName, StringComparison.InvariantCultureIgnoreCase));
        if (envIndex < 0)
        {
            return;
        }
        var env = _availableEnvironments[envIndex];
        _availableEnvironments[envIndex] = new Environment(env.CustomPath, customEnv);
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