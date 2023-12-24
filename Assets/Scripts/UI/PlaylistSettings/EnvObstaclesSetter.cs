using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvObstaclesSetter : EnvironmentAssetSetter
{
    public override string GetPlaylistAssetName(Playlist playlist)
    {
        SaveLog("Obstacles: Getting Playlist Gloves Name");
        SaveLog($"Obstacles: Playlist: {playlist}");
        if (playlist != null)
        {
            SaveLog($"Obstacles: Playlist Name: {playlist.PlaylistName}");
            SaveLog($"Obstacles: Playlist Obstacles: {playlist.Obstacles}");

            if (playlist.Obstacles != null)
            {
                SaveLog($"Trying to get obstacles name");
                SaveLog($"Obstacles: Playlist Obstacles Name: {playlist.Obstacles.AssetName}");
            }
        }
        return playlist?.TargetEnvObstaclesName;
    }

    public override string GetEnvAssetName(Environment environment)
    {
        return environment.ObstaclesName;
    }

    protected override bool ShouldUpdateFromEnv()
    {
        if (_ignorePlaylists)
        {
            return true;
        }
        var managerNull = PlaylistManager.Instance == null;
        var playlistNull = managerNull || PlaylistManager.Instance.CurrentPlaylist == null;
        var obstaclesNull = managerNull || playlistNull || PlaylistManager.Instance.CurrentPlaylist.Obstacles == null;

        return _ignorePlaylists || obstaclesNull;
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

    public override EnvAssetReference GetAssetRef(int index)
    {
        return EnvironmentControlManager.Instance.GetObstacleAtIndex(index);
    }

    protected override string GetAssetName(Playlist sourcePlaylist)
    {
        var assetName = GetPlaylistAssetName(sourcePlaylist);
        if(sourcePlaylist == null)
        {
            return assetName;
        }

        if (EnvironmentControlManager.Instance != null && (sourcePlaylist.Obstacles == null || string.IsNullOrWhiteSpace(sourcePlaylist.Obstacles.AssetName)))
        {
            if(!string.IsNullOrWhiteSpace(sourcePlaylist.TargetEnvName) && EnvironmentControlManager.Instance.TryGetEnvRefByName(sourcePlaylist.TargetEnvName, out var environment) && environment.Obstacles != null)
            {
                assetName = environment.ObstaclesName;
                return assetName;
            }
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
        if (playlist == null)
        {
            return;
        }
        if (playlist.Obstacles != null)
        {
            EnvironmentControlManager.Instance.SetObstaclesOverride(playlist.Obstacles);
        }
    }

    protected override void ResetOverrides()
    {
        EnvironmentControlManager.Instance.SetObstaclesOverride(null);
    }
}
