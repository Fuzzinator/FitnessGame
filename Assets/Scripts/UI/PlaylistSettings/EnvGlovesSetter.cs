using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvGlovesSetter : EnvironmentAssetSetter
{
    public override string GetPlaylistAssetName(Playlist playlist)
    {
        SaveLog("Gloves: Getting Playlist Gloves Name");
        SaveLog($"Gloves: Playlist: {playlist}");
        if (playlist != null)
        {
            SaveLog($"Gloves: Playlist Name: {playlist.PlaylistName}");
            SaveLog($"Gloves: Playlist Gloves: {playlist.Gloves}");
            if (playlist.Gloves != null)
            {
                SaveLog($"Trying to get glove name");
                SaveLog($"Gloves: Playlist Gloves Name: {playlist.Gloves.AssetName}");
            }
        }
        return playlist != null ? playlist.TargetEnvGlovesName : null;
    }

    public override string GetEnvAssetName(Environment environment)
    {
        return environment.GlovesName;
    }
    protected override bool ShouldUpdateFromEnv()
    {
        if (_ignorePlaylists)
        {
            return true;
        }
        var managerNull = PlaylistManager.Instance == null;
        var playlistNull = managerNull || PlaylistManager.Instance.CurrentPlaylist == null;
        var glovesNull = managerNull || playlistNull || PlaylistManager.Instance.CurrentPlaylist.Gloves == null;

        return _ignorePlaylists || glovesNull;
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

    public override EnvAssetReference GetAssetRef(int index)
    {
        return EnvironmentControlManager.Instance.GetGloveAtIndex(index);
    }
    protected override string GetAssetName(Playlist sourcePlaylist)
    {
        SaveLog("Getting Gloves Name");
        var assetName = GetPlaylistAssetName(sourcePlaylist);
        if (sourcePlaylist == null)
        {
            return assetName;
        }

        SaveLog("Gloves: Playlist Not Null");
        if (EnvironmentControlManager.Instance != null && (sourcePlaylist.Gloves == null || string.IsNullOrWhiteSpace(sourcePlaylist.Gloves.AssetName)))
        {
            SaveLog("Trying To Get Glove Ref by name");
            if (!string.IsNullOrWhiteSpace(sourcePlaylist.TargetEnvName) && EnvironmentControlManager.Instance.TryGetEnvRefByName(sourcePlaylist.TargetEnvName, out var environment) && environment.Gloves != null)
            {
                SaveLog("Got glove ref by name");
                assetName = environment.GlovesName;
                return assetName;
            }

            SaveLog("Glove: Getting Active Env Container");
            var currentEnv = EnvironmentControlManager.Instance.ActiveEnvironmentContainer;

            SaveLog("Gloves: Checking if null");
            if (currentEnv.Gloves != null)
            {
                SaveLog("Gloves: getting glove name from env");
                assetName = currentEnv.Gloves.GlovesName;
            }
            else
            {
                SaveLog("Gloves: getting asset 0");
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

    protected override void ResetOverrides()
    {
        EnvironmentControlManager.Instance.SetGloveOverride(null);
    }
}
