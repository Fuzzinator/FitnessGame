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

    }

    protected override int GetAssetIndex()
    {
        return 0;
    }
}
