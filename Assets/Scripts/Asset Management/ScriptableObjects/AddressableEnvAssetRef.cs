using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "New Asset Reference", menuName = "ScriptableObjects/Environment Asset Reference", order = 1)]
public class AddressableEnvAssetRef : ScriptableObject
{
    [SerializeField]
    private string _environmentName;

    [field: SerializeField]
    public Sprite Thumbnail { get; private set; }

    [field: SerializeField]
    public EnvAssetRef Scene { get; private set; }

    [field: SerializeField]
    public EnvAssetRef Gloves { get; private set; }

    [field: SerializeField]
    public EnvAssetRef Targets { get; private set; }

    [field: SerializeField]
    public EnvAssetRef Obstacles { get; private set; }

    [SerializeField]
    private TargetPlatform _targetPlatform;

    public string EnvironmentName => _environmentName;

    public EnvSceneRef SceneAssets => (EnvSceneRef)Scene.AssetPath.Asset;

    public string GlovesName => Gloves != null ? Gloves.AssetName : null;

    public EnvGlovesRef GloveAssets => Gloves.AssetPath.Asset as EnvGlovesRef;

    public string TargetsName => Targets != null ? Targets.AssetName : null;

    public EnvTargetsRef TargetsAssets => Targets.AssetPath.Asset as EnvTargetsRef;

    public string ObstaclesName => Obstacles != null ? Obstacles.AssetName : null;

    public EnvObstaclesRef ObstaclesAssets => Obstacles.AssetPath.Asset as EnvObstaclesRef;

    public TargetPlatform TargetPlatform => _targetPlatform;
}

public struct EnvAssets
{
    [field: SerializeField]
    public string EnvironmentName { get; private set; }
    
    [field: SerializeField]
    public EnvAssetRef Scene { get; private set; }

    [field: SerializeField]
    public EnvAssetReference Gloves { get; private set; }

    [field: SerializeField]
    public EnvAssetReference Targets { get; private set; }

    [field: SerializeField]
    public EnvAssetReference Obstacles { get; private set; }

    public EnvAssets(AddressableEnvAssetRef asset)
    {
        EnvironmentName = asset.EnvironmentName;
        Scene = asset.Scene;
        Gloves = new EnvAssetReference(asset.Gloves);
        Targets = new EnvAssetReference(asset.Targets);
        Obstacles = new EnvAssetReference(asset.Obstacles);
    }

    public EnvAssets(string envName, EnvAssetRef scene, EnvAssetReference gloves, EnvAssetReference targets, EnvAssetReference obstacles)
    {
        EnvironmentName = envName;
        Scene = scene;
        Gloves = gloves;
        Targets = targets;
        Obstacles = obstacles;
    }
}