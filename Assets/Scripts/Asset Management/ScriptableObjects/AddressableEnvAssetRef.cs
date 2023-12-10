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

    public string GlovesName => Gloves?.AssetName;

    public EnvGlovesRef GloveAssets => (EnvGlovesRef)Gloves.AssetPath.Asset;

    public string TargetsName => Targets?.AssetName;

    public EnvTargetsRef TargetsAssets => (EnvTargetsRef)Targets.AssetPath.Asset;

    public string ObstaclesName => Obstacles?.AssetName;

    public EnvObstaclesRef ObstaclesAssets => (EnvObstaclesRef)Obstacles.AssetPath.Asset;

    public TargetPlatform TargetPlatform => _targetPlatform;
}

public struct EnvAssets
{
    [field: SerializeField]
    public string EnvironmentName { get; private set; }
    
    [field: SerializeField]
    public EnvAssetRef Scene { get; private set; }

    [field: SerializeField]
    public EnvAssetRef Gloves { get; private set; }

    [field: SerializeField]
    public EnvAssetRef Targets { get; private set; }

    [field: SerializeField]
    public EnvAssetRef Obstacles { get; private set; }

    public EnvAssets(AddressableEnvAssetRef asset)
    {
        EnvironmentName = asset.EnvironmentName;
        Scene = asset.Scene;
        Gloves = asset.Gloves;
        Targets = asset.Targets;
        Obstacles = asset.Obstacles;
    }

    public EnvAssets(string envName, EnvAssetRef scene, EnvAssetRef gloves, EnvAssetRef targets, EnvAssetRef obstacles)
    {
        EnvironmentName = envName;
        Scene = scene;
        Gloves = gloves;
        Targets = targets;
        Obstacles = obstacles;
    }
}