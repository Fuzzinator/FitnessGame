using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvObstaclesSetter : EnvironmentAssetSetter
{
    public override string GetPlaylistAssetName(Playlist playlist)
    {
        return playlist?.TargetEnvObstaclesName;
    }

    public override string GetEnvAssetName(Environment environment)
    {
        return environment.ObstaclesName;
    }
    protected override bool ShouldUpdateFromEnv()
    {
        return PlaylistManager.Instance?.CurrentPlaylist?.Obstacles == null;
    }

    public override void SetAssetIndex(int index)
    {
        var obstaclesName = EnvironmentControlManager.Instance.SetObstacleOverride(index);
        SetText(obstaclesName?.AssetName);
        DisableOptionsDisplay();
    }

    protected override int GetAssetIndex()
    {
        return 0;
    }

    public override int GetAvailableAssetCount()
    {
        return EnvironmentControlManager.Instance.AvailableObstacleCount;
    }

    public override EnvAssetRef GetAssetRef(int index)
    {
        return EnvironmentControlManager.Instance.GetObstacleAtIndex(index);
    }

    protected override string GetAssetName(Playlist sourcePlaylist)
    {
        var assetName = GetPlaylistAssetName(sourcePlaylist);
        if (string.IsNullOrWhiteSpace(sourcePlaylist.Obstacles?.AssetName) && EnvironmentControlManager.Instance != null)
        {
            var currentEnv = EnvironmentControlManager.Instance.ActiveEnvironmentContainer;
            if (currentEnv.Obstacles != null)
            {
                assetName = currentEnv.Obstacles.ObstaclesName;
            }
            else
            {
                assetName = GetAssetFromEnvIndex(0).ObstaclesName;
            }
        }
        return assetName;
    }

    protected override void TrySetAsset(Playlist playlist)
    {
        if (playlist.Obstacles != null)
        {
            EnvironmentControlManager.Instance.SetObstaclesOverride(playlist.Obstacles);
        }
    }
}
