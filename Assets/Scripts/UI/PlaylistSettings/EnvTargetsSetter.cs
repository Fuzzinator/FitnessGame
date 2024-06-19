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
        if (_ignorePlaylists)
        {
            return true;
        }

        var managerNull = PlaylistManager.Instance == null;
        var playlistNull = managerNull || PlaylistManager.Instance.CurrentPlaylist == null;
        var targetsNull = managerNull || playlistNull || PlaylistManager.Instance.CurrentPlaylist.Targets == null;

        return _ignorePlaylists || targetsNull;
    }

    public override void SetAssetIndex(int index)
    {
        var targetsName = EnvironmentControlManager.Instance.SetDefaultTargetOverride(index);
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

    public override EnvAssetReference GetAssetRef(int index)
    {
        return EnvironmentControlManager.Instance.GetTargetAtIndex(index);
    }

    protected override string GetAssetName(Playlist sourcePlaylist)
    {
        var assetName = GetPlaylistAssetName(sourcePlaylist);
        if (sourcePlaylist == null)
        {
            return assetName;
        }

        if (EnvironmentControlManager.Instance != null && (sourcePlaylist.Targets == null || string.IsNullOrWhiteSpace(sourcePlaylist.Targets.AssetName)))
        {
            if (!string.IsNullOrWhiteSpace(sourcePlaylist.TargetEnvName) && EnvironmentControlManager.Instance.TryGetEnvRefByName(sourcePlaylist.TargetEnvName, out var environment) && environment.Targets != null)
            {
                if (EnvironmentControlManager.Instance.TargetsOverride != null)
                {
                    assetName = EnvironmentControlManager.Instance.TargetsOverride.AssetName;
                }
                else
                {
                    assetName = environment.TargetsName;
                }
                return assetName;
            }

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
        if (playlist == null)
        {
            return;
        }

        if (playlist.Targets != null)
        {
            EnvironmentControlManager.Instance.SetTargetOverride(playlist.Targets);
        }
    }

    protected override void ResetOverrides()
    {
        EnvironmentControlManager.Instance.ResetTargets();
    }

    protected override void GetAndSetText()
    {
        var targets = EnvironmentControlManager.Instance.TargetsOverride;
        if (_ignorePlaylists && targets != null && !string.IsNullOrWhiteSpace(targets.AssetName))
        {
            SetText(targets.AssetName);
        }
        else
        {
            var targetEnv = EnvironmentControlManager.Instance.GetTargetEnvironment();
            if (string.IsNullOrWhiteSpace(targetEnv.TargetsName))
            {
                targetEnv = EnvironmentControlManager.Instance.GetCustomEnvironment();
            }
            if (string.IsNullOrWhiteSpace(targetEnv.TargetsName))
            {
                base.GetAndSetText();
                return;
            }
            SetText(targetEnv.TargetsName);
        }
    }

    protected override bool CheckForOverrideName(out string overrideName)
    {
        overrideName = EnvironmentControlManager.Instance.TargetsOverride?.AssetName;
        return !string.IsNullOrWhiteSpace(overrideName);
    }
}
