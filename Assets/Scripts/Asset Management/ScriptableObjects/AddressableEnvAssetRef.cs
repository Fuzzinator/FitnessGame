using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "New Asset Reference", menuName = "ScriptableObjects/Environment Asset Reference",
    order = 2)]
public class AddressableEnvAssetRef : ScriptableObject
{
    [SerializeField]
    private string _environmentName;

    [SerializeField]
    private AssetReference _assetReference;

    [SerializeField]
    private TargetPlatform _targetPlatform;

    public string EnvironmentName => _environmentName;
    public AssetReference AssetReference => _assetReference;

    public TargetPlatform TargetPlatform => _targetPlatform;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(_environmentName))
            _environmentName = ((EnvironmentAssetContainer) AssetReference.editorAsset).EnvironmentName;
    }
#endif    
}