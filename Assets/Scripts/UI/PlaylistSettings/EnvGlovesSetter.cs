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
        var gloveName = EnvironmentControlManager.Instance.SetGloveOverride(index);
        SetText(gloveName?.AssetName);
        DisableOptionsDisplay();
    }

    protected override int GetAssetIndex()
    {
        return 0;
    }

    public override int GetAvailableAssetCount()
    {
        return EnvironmentControlManager.Instance.AvailableGloveCount;
    }

    public override EnvAssetRef GetAssetRef(int index)
    {
        return EnvironmentControlManager.Instance.GetGloveAtIndex(index);
    }
    protected override string GetAssetName(Playlist sourcePlaylist)
    {
        var assetName = GetPlaylistAssetName(sourcePlaylist);
        if (string.IsNullOrWhiteSpace(sourcePlaylist?.Gloves?.AssetName) && EnvironmentControlManager.Instance != null)
        {
            var currentEnv = EnvironmentControlManager.Instance.ActiveEnvironmentContainer;
            if (currentEnv.Gloves != null)
            {
                assetName = currentEnv.Gloves.GlovesName;
            }
            else
            {
                assetName = GetAssetFromEnvIndex(0).GlovesName;
            }
        }
        return assetName;
    }

    protected override void TrySetAsset(Playlist playlist)
    {
        if (playlist.Gloves != null)
        {
            EnvironmentControlManager.Instance.SetGloveOverride(playlist.Gloves);
        }
    }
}
