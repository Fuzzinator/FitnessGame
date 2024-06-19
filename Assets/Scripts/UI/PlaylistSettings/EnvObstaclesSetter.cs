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
        var obstaclesName = EnvironmentControlManager.Instance.SetDefaultObstacleOverride(index);
        SetText(obstaclesName?.AssetName);
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
        if (sourcePlaylist == null)
        {
            return assetName;
        }

        if (EnvironmentControlManager.Instance != null && (sourcePlaylist.Obstacles == null || string.IsNullOrWhiteSpace(sourcePlaylist.Obstacles.AssetName)))
        {
            if (!string.IsNullOrWhiteSpace(sourcePlaylist.TargetEnvName) && EnvironmentControlManager.Instance.TryGetEnvRefByName(sourcePlaylist.TargetEnvName, out var environment) && environment.Obstacles != null)
            {
                if (EnvironmentControlManager.Instance.ObstaclesOverride != null)
                {
                    assetName = EnvironmentControlManager.Instance.ObstaclesOverride.AssetName;
                }
                else
                {
                    assetName = environment.ObstaclesName;
                }
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
        EnvironmentControlManager.Instance.ResetObstacles();
    }

    protected override void GetAndSetText()
    {
        var obstacles = EnvironmentControlManager.Instance.ObstaclesOverride;
        if (_ignorePlaylists && obstacles != null && !string.IsNullOrWhiteSpace(obstacles.AssetName))
        {
            SetText(obstacles.AssetName);
        }
        else
        {
            var targetEnv = EnvironmentControlManager.Instance.GetTargetEnvironment();
            if (string.IsNullOrWhiteSpace(targetEnv.ObstaclesName))
            {
                targetEnv = EnvironmentControlManager.Instance.GetCustomEnvironment();
            }
            if (string.IsNullOrWhiteSpace(targetEnv.ObstaclesName))
            {
                base.GetAndSetText();
                return;
            }
            SetText(targetEnv.ObstaclesName);
        }
    }

    protected override bool CheckForOverrideName(out string overrideName)
    {
        overrideName = EnvironmentControlManager.Instance.ObstaclesOverride?.AssetName;
        return !string.IsNullOrWhiteSpace(overrideName);
    }
}
