using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewEnvObstacleSetter : NewEnvAssetSetter
{
    public override EnvAssetReference GetAssetRef(int index)
    {
        return EnvironmentControlManager.Instance.GetObstacleAtIndex(index);
    }

    public override int GetAvailableAssetCount()
    {
        return EnvironmentControlManager.Instance.AvailableObstacleCount;
    }

    public override void SetAssetIndex(int index)
    {
        var obstaclesAsset = GetAssetRef(index);
        _envCreator.SetObstacles(obstaclesAsset);
        SetText(obstaclesAsset?.AssetName);
    }

    protected override void GetAndSetText()
    {
        var obstaclesAsset = _envCreator.ObstaclesRef ?? GetAssetRef(0);
        _envCreator.SetObstacles(obstaclesAsset);
        SetText(obstaclesAsset?.AssetName);
    }
}
