
public class NewPlaylistTargetSetter : NewPlaylistEnvAssetSetter
{
    public override EnvAssetReference GetAssetRef(int assetIndex)
    {
        if (assetIndex < 0 || assetIndex >= GetAvailableAssetCount())
        {
            return EnvironmentControlManager.Instance.GetTargetAtIndex(0);
        }
        return EnvironmentControlManager.Instance.GetTargetAtIndex(assetIndex);
    }

    public override int GetAvailableAssetCount()
    {
        return EnvironmentControlManager.Instance.AvailableTargetCount;
    }

    public override void SetAssetIndex(int index)
    {
        var assetRef = GetAssetRef(index);
        PlaylistMaker.Instance.SetTargets(assetRef);
        SetText(assetRef.AssetName);
    }

    protected override string GetAssetName()
    {
        return PlaylistMaker.Instance.TargetsName;
    }

    protected override string GetDefaultAssetName()
    {
        var hasRef = EnvironmentControlManager.Instance.TryGetEnvRefAtIndex(0, out var assetRef);
        if (!hasRef || string.IsNullOrWhiteSpace(assetRef.TargetsName))
        {
            return EnvironmentControlManager.Instance.GetTargetAtIndex(0).AssetName;
        }
        return assetRef.TargetsName;
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

        var assetName = assetRef.TargetsName;
        if (string.IsNullOrWhiteSpace(assetName))
        {
            if (EnvironmentControlManager.Instance.TryGetEnvRefByName("Sci-Fi Arena", out var envRef))
            {
                assetName = envRef.TargetsName;
                PlaylistMaker.Instance.SetTargets(envRef.Targets);
            }
            else
            {
                var targets = EnvironmentControlManager.Instance.GetTargetAtIndex(0);
                assetName = targets.AssetName;

                PlaylistMaker.Instance.SetTargets(targets);
            }
        }
        else
        {
            PlaylistMaker.Instance.SetTargets(assetRef.Targets);
        }

        SetText(assetName);
    }
}
