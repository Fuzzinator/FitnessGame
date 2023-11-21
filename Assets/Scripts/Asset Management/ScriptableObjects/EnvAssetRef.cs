using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
[CreateAssetMenu(fileName = "New Env Gloves Ref", menuName = "ScriptableObjects/Env Asset Refs/Env Gloves Asset Ref", order = 5)]
public class EnvAssetRef : ScriptableObject
{
    [field: SerializeField]
    public string AssetName { get; private set; }

    [field: SerializeField]
    public AssetReference SpriteThumbnail { get; private set; }

    [field: SerializeField]
    public AssetReference AssetPath { get; private set; }
}
