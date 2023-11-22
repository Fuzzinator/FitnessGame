using UnityEngine;
using UnityEngine.AddressableAssets;
using static BeatsaberV3Choreography;

public struct EnvironmentAssetContainer
{
    [SerializeField]
    private string _environmentName;

    [field: SerializeField]
    public EnvSceneRef Scene { get; private set; }

    [field: SerializeField]
    public EnvGlovesRef Gloves { get; private set; }

    [field: SerializeField]
    public EnvTargetsRef Targets { get; private set; }

    [field: SerializeField]
    public EnvObstaclesRef Obstacles { get; private set; }

    public string EnvironmentName => _environmentName;

    public AssetReference SceneAsset => Scene.SceneAsset;
    public Texture2DArray TargetTextures => Targets.TargetsTexture;
    public Texture2DArray ObstacleTextures => Obstacles.ObstacleTexture;
    public TextureSet[] GlobalTextureSets => Scene.GlobalTextureSets;
    public TextureArraySet[] GlobalTextureArraySets => Scene.GlobalTextureArraySets;

    public GloveController LeftGlove => Gloves.LeftGlove;
    public GloveController RightGlove => Gloves.RightGlove;
    public BaseTarget JabTarget => Targets.JabTarget;
    public BaseTarget HookLeftTarget => Targets.HookLeftTarget;
    public BaseTarget HookRightTarget => Targets.HookRightTarget;
    public BaseTarget UppercutTarget => Targets.UppercutTarget;
    public BlockTarget BlockTarget => Targets.BlockTarget;
    public BaseObstacle DuckObstacle => Obstacles.DuckObstacle;
    public BaseObstacle DodgeLeftObstacle => Obstacles.DodgeLeftObstacle;
    public BaseObstacle DodgeRightObstacle => Obstacles.DodgeRightObstacle;
    public BaseHitVFX BaseHitVFX => Targets.HitVFX;
    public Material TargetMaterial => Targets.TargetsMaterial;
    public Material SuperTargetMaterial => Targets.SuperTargetMaterial;
    public Material ObstacleMaterial => Obstacles.ObstacleMaterial;

    public EnvironmentAssetContainer(string environmentName, EnvSceneRef scene, EnvGlovesRef gloves, EnvTargetsRef targets, EnvObstaclesRef obstacles)
    {
        _environmentName = environmentName;
        Scene = scene;
        Gloves = gloves;
        Targets = targets;
        Obstacles = obstacles;
    }
}
