using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvTargetsSetter : EnvironmentAssetSetter
{
    public override string GetPlaylistAssetName(Playlist playlist)
    {
        return playlist?.TargetEnvTargetsName;
    }

    public override string GetEnvAssetName(Environment environment)
    {
        return environment.TargetsName;
    }

    protected override bool ShouldUpdateFromEnv()
    {
        return PlaylistManager.Instance?.CurrentPlaylist?.Targets == null;
    }

    public override void SetAssetIndex(int index)
    {

    }

    protected override int GetAssetIndex()
    {
        return 0;
    }
}
