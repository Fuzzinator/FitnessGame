using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvGlovesSetter : EnvironmentAssetSetter
{
    public override string GetPlaylistAssetName(Playlist playlist)
    {
        return playlist?.TargetEnvGlovesName;
    }

    public override string GetEnvAssetName(Environment environment)
    {
        return environment.GlovesName;
    }
    protected override bool ShouldUpdateFromEnv()
    {
        return PlaylistManager.Instance?.CurrentPlaylist?.Gloves == null;
    }

    public override void SetAssetIndex(int index)
    {
        EnvironmentControlManager.Instance.SetGloveOverride(index);
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
