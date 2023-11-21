using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "New Asset Reference", menuName = "ScriptableObjects/Environment Asset Reference",
    order = 2)]
public class AddressableEnvAssetRef : ScriptableObject
{
    [SerializeField]
    private string _environmentName;

    [field: SerializeField]
    public Sprite Thumbnail { get; private set; }

    [SerializeField]
    private AssetReference _assetReference;

    [field: SerializeField]
    public EnvAssetRef Gloves { get; private set; }

    [field: SerializeField]
    public EnvAssetRef Targets { get; private set; }

    [field: SerializeField]
    public EnvAssetRef Obstacles { get; private set; }

    [SerializeField]
    private TargetPlatform _targetPlatform;

    public string EnvironmentName => _environmentName;
    public AssetReference AssetReference => _assetReference;

    public string GlovesName => Gloves?.AssetName;

    public string TargetsName => Targets?.AssetName;

    public string ObstaclesName => Obstacles?.AssetName;

    public TargetPlatform TargetPlatform => _targetPlatform;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(_environmentName))
            _environmentName = ((EnvironmentAssetContainer) AssetReference.editorAsset).EnvironmentName;
    }
#endif    
}