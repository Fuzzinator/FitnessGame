using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewEnvGloveSetter : NewEnvAssetSetter
{
    public override EnvAssetReference GetAssetRef(int index)
    {
        return EnvironmentControlManager.Instance.GetGloveAtIndex(index);
    }

    public override int GetAvailableAssetCount()
    {
        return EnvironmentControlManager.Instance.AvailableGloveCount;
    }

    public override void SetAssetIndex(int index)
    {
        var gloveAsset = GetAssetRef(index);
        _envCreator.SetGloves(gloveAsset);
        SetText(gloveAsset?.AssetName);
    }

    protected override void GetAndSetText()
    {
        var gloveAsset = _envCreator.GlovesRef ?? GetAssetRef(0);
        _envCreator.SetGloves(gloveAsset);
        SetText(gloveAsset?.AssetName);
    }
}
