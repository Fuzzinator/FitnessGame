
public class NewPlaylistObstacleSetter : NewPlaylistEnvAssetSetter
{
    public override EnvAssetRef GetAssetRef(int assetIndex)
    {
        if (assetIndex < 0 || assetIndex >= GetAvailableAssetCount())
        {
            return EnvironmentControlManager.Instance.GetObstacleAtIndex(0);
        }
        return EnvironmentControlManager.Instance.GetObstacleAtIndex(assetIndex);
    }

    public override int GetAvailableAssetCount()
    {
        return EnvironmentControlManager.Instance.AvailableObstacleCount;
    }

    public override void SetAssetIndex(int index)
    {
        var assetRef = GetAssetRef(index);
        PlaylistMaker.Instance.SetObstacles(assetRef);
        SetText(assetRef.AssetName);
    }

    protected override string GetAssetName()
    {
        return PlaylistMaker.Instance.ObstaclesName;
    }

    protected override string GetDefaultAssetName()
    {
        var hasRef = EnvironmentControlManager.Instance.TryGetEnvRefAtIndex(0, out var assetRef);
        if (!hasRef || string.IsNullOrWhiteSpace(assetRef.ObstaclesName))
        {
            return EnvironmentControlManager.Instance.GetObstacleAtIndex(0).AssetName;
        }
        return assetRef.ObstaclesName;
    }


    protected override void UpdateFromEnvIndexChange(int index)
    {
        if (_assetOverride != null)
        {
            return;
        }

        var hasRef = EnvironmentControlManager.Instance.TryGetEnvRefAtIndex(index, out var assetRef);
        if (!hasRef)
        {
            return;
        }

        var assetName = assetRef.ObstaclesName;
        if (string.IsNullOrWhiteSpace(assetName))
        {
            if (EnvironmentControlManager.Instance.TryGetEnvRefByName("Sci-Fi Arena", out var envRef))
            {
                assetName = envRef.ObstaclesName;
                PlaylistMaker.Instance.SetObstacles(envRef.Obstacles);
            }
            else
            {
                var obtsacles = EnvironmentControlManager.Instance.GetObstacleAtIndex(0);
                assetName = obtsacles.AssetName;

                PlaylistMaker.Instance.SetObstacles(obtsacles);
            }
        }
        else
        {
            PlaylistMaker.Instance.SetObstacles(assetRef.Obstacles);
        }

        SetText(assetName);
    }
}
