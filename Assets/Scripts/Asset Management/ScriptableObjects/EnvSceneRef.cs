using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "New Env Scene Ref", menuName = "ScriptableObjects/Env Asset Refs/Env Scene Ref", order = 4)]
public class EnvSceneRef : ScriptableObject
{
    [field: SerializeField]
    public string TargetsName { get; private set; }
    [field: SerializeField]
    public Sprite Thumbnail { get; private set; }

    [field: SerializeField]
    public AssetReference SceneAsset { get; private set; }

    [field: SerializeField]
    public TextureSet[] GlobalTextureSets { get; private set; }

    [field: SerializeField]
    public TextureArraySet[] GlobalTextureArraySets { get; private set; }

}
