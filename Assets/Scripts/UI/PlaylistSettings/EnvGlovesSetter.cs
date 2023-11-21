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

    }

    protected override int GetAssetIndex()
    {
        return 0;
    }
}
