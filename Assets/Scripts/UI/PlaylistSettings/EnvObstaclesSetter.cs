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
        EnvironmentControlManager.Instance.SetObstacleOverride(index);
    }

    protected override int GetAssetIndex()
    {
        return 0;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EnvironmentControlManager.Instance.SetObstacleOverride(-1);
    }
}
