using System;
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
        var targetsName = EnvironmentControlManager.Instance.SetTargetOverride(index);
        SetText(targetsName?.AssetName);
    }

    protected override int GetAssetIndex()
    {
        return 0;
    }

    public override int GetAvailableAssetCount()
    {
        return EnvironmentControlManager.Instance.AvailableTargetCount;
    }

    public override EnvAssetRef GetAssetRef(int index)
    {
        return EnvironmentControlManager.Instance.GetTargetAtIndex(index);
    }

    protected override string GetAssetName(Playlist sourcePlaylist)
    {
        var assetName = GetPlaylistAssetName(sourcePlaylist);
        if (string.IsNullOrWhiteSpace(sourcePlaylist.Targets?.AssetName) && EnvironmentControlManager.Instance != null)
        {
            var currentEnv = EnvironmentControlManager.Instance.ActiveEnvironmentContainer;
            if (currentEnv.Targets != null)
            {
                assetName = currentEnv.Targets.TargetsName;
            }
            else
            {
                assetName = GetAssetFromEnvIndex(0).TargetsName;
            }
        }
        return assetName;
    }

    protected override void TrySetAsset(Playlist playlist)
    {
        if (playlist.Targets != null)
        {
            EnvironmentControlManager.Instance.SetTargetOverride(playlist.Targets);
        }
    }
}
