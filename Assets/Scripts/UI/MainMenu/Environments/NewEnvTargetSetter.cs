using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewEnvTargetSetter : NewEnvAssetSetter
{
    public override EnvAssetReference GetAssetRef(int index)
    {
        return EnvironmentControlManager.Instance.GetTargetAtIndex(index);
    }

    public override int GetAvailableAssetCount()
    {
        return EnvironmentControlManager.Instance.AvailableTargetCount;
    }

    public override void SetAssetIndex(int index)
    {
        var targetAsset = GetAssetRef(index);
        _envCreator.SetTargets(targetAsset);
        SetText(targetAsset?.AssetName);
    }

    protected override void GetAndSetText()
    {
        var targetAsset = _envCreator.TargetsRef ?? GetAssetRef(0);
        _envCreator.SetTargets(targetAsset);
        SetText(targetAsset?.AssetName);
    }
}
