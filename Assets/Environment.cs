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

    public bool IsValid { get; private set; }

    public Environment(string customLocation)
    {
        Name = Path.GetFileNameWithoutExtension(customLocation);
        IsCustom = true;
        CustomPath = customLocation;
        AssetRef = null;
        IsValid = true;
    }

    public Environment(AddressableEnvAssetRef assetRef)
    {
        Name = assetRef.EnvironmentName;
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