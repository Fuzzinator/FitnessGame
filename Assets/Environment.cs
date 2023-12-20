using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct Environment
{
    public string Name { get; private set; }
    public bool IsCustom { get; private set; }

    public string CustomPath { get; private set; }
    public AddressableEnvAssetRef AssetRef { get; private set; }

    [field: SerializeField]
    public EnvAssetReference Gloves { get; private set; }

    [field: SerializeField]
    public EnvAssetReference Targets { get; private set; }

    [field: SerializeField]
    public EnvAssetReference Obstacles { get; private set; }

    public string GlovesName => Gloves?.AssetName;

    public string TargetsName => Targets?.AssetName;

    public string ObstaclesName => Obstacles?.AssetName;

    public bool IsValid { get; private set; }

    public Environment(string customLocation, CustomEnvironment customEnv)
    {
        Name = customEnv.EnvironmentName;//Path.GetFileNameWithoutExtension(customLocation);
        Gloves = customEnv.Gloves;
        Targets = customEnv.Targets;
        Obstacles = customEnv.Obstacles;
        IsCustom = true;
        CustomPath = customLocation;
        AssetRef = null;
        IsValid = true;
    }

    public Environment(AddressableEnvAssetRef assetRef)
    {
        Name = assetRef.EnvironmentName;
        Gloves = new(assetRef.Gloves);
        Targets = new(assetRef.Targets);
        Obstacles = new(assetRef.Obstacles);
        IsCustom = false;
        CustomPath = null;
        AssetRef = assetRef;
        IsValid = true;
    }
}

public enum TargetPlatform
{
    All = 0,
    Android = 1,
    PCVR = 2,
    CustomEnvironment = 3,
}