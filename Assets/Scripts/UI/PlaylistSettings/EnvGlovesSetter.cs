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
        var gloveName = EnvironmentControlManager.Instance.SetDefaultGloveOverride(index);
        SetText(gloveName?.AssetName);
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
        var assetName = GetPlaylistAssetName(sourcePlaylist);
        if (sourcePlaylist == null)
        {
            return assetName;
        }

        if (EnvironmentControlManager.Instance != null && (sourcePlaylist.Gloves == null || string.IsNullOrWhiteSpace(sourcePlaylist.Gloves.AssetName)))
        {
            if (!string.IsNullOrWhiteSpace(sourcePlaylist.TargetEnvName) && EnvironmentControlManager.Instance.TryGetEnvRefByName(sourcePlaylist.TargetEnvName, out var environment) && environment.Gloves != null)
            {
                if (EnvironmentControlManager.Instance.GlovesOverride != null)
                {
                    assetName = EnvironmentControlManager.Instance.GlovesOverride.AssetName;
                }
                else
                {
                    assetName = environment.GlovesName;
                }
                return assetName;
            }

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
        if (playlist == null)
        {
            return;
        }

        if (playlist.Gloves != null)
        {
            EnvironmentControlManager.Instance.SetGloveOverride(playlist.Gloves);
        }
    }

    protected override void ResetOverrides()
    {
        EnvironmentControlManager.Instance.ResetGloves();
    }

    protected override void GetAndSetText()
    {
        var gloves = EnvironmentControlManager.Instance.GlovesOverride;
        if (_ignorePlaylists && gloves != null && !string.IsNullOrWhiteSpace(gloves.AssetName))
        {
            SetText(gloves.AssetName);
        }
        else
        {
            var targetEnv = EnvironmentControlManager.Instance.GetTargetEnvironment();
            if (string.IsNullOrWhiteSpace(targetEnv.GlovesName))
            {
                targetEnv = EnvironmentControlManager.Instance.GetCustomEnvironment();
            }
            if (string.IsNullOrWhiteSpace(targetEnv.GlovesName))
            {
                base.GetAndSetText();
                return;
            }
            SetText(targetEnv.GlovesName);
        }
    }

    protected override bool CheckForOverrideName(out string overrideName)
    {
        overrideName = EnvironmentControlManager.Instance.GlovesOverride?.AssetName;
        return !string.IsNullOrWhiteSpace(overrideName);
    }
}
