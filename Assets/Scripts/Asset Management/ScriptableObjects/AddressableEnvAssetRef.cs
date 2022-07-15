using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "New Asset Reference", menuName = "ScriptableObjects/Environment Asset Reference", order = 2)]
public class AddressableEnvAssetRef : ScriptableObject
{
    [SerializeField]
    private string _environmentName;
    [SerializeField]
    private AssetReference _assetReference;

    public string EnvironmentName => _environmentName;
    public AssetReference AssetReference => _assetReference;
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        _environmentName = ((EnvironmentAssetContainer) AssetReference.editorAsset).EnvironmentName;
    }
#endif
}
