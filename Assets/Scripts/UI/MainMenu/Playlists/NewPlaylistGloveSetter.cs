
using System;

public class NewPlaylistGloveSetter : NewPlaylistEnvAssetSetter
{
    public override EnvAssetReference GetAssetRef(int assetIndex)
    {
        if (assetIndex < 0 || assetIndex >= GetAvailableAssetCount())
        {
            return EnvironmentControlManager.Instance.GetGloveAtIndex(0);
        }
        return EnvironmentControlManager.Instance.GetGloveAtIndex(assetIndex);
    }

    public override int GetAvailableAssetCount()
    {
        return EnvironmentControlManager.Instance.AvailableGloveCount;
    }

    public override void SetAssetIndex(int index)
    {
        var assetRef = GetAssetRef(index);
        _assetOverride = assetRef;
        PlaylistMaker.Instance.SetGloves(assetRef);
        SetText(assetRef.AssetName);
    }

    protected override string GetAssetName()
    {
        return PlaylistMaker.Instance.GlovesName;
    }

    protected override string GetDefaultAssetName()
    {
        var hasRef = EnvironmentControlManager.Instance.TryGetEnvRefAtIndex(0, out var assetRef);
        if (!hasRef || string.IsNullOrWhiteSpace(assetRef.GlovesName))
        {
            return EnvironmentControlManager.Instance.GetGloveAtIndex(0).AssetName;
        }
        return assetRef.GlovesName;
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
        var assetName = assetRef.GlovesName;
        if (string.IsNullOrWhiteSpace(assetName))
        {
            if (EnvironmentControlManager.Instance.TryGetEnvRefByName("Sci-Fi Arena", out var envRef))
            {
                assetName = envRef.GlovesName;
                PlaylistMaker.Instance.SetGloves(envRef.Gloves);
            }
            else
            {
                var gloves = EnvironmentControlManager.Instance.GetGloveAtIndex(0);
                assetName = gloves.AssetName;

                PlaylistMaker.Instance.SetGloves(gloves);
            }
        }
        else
        {
            PlaylistMaker.Instance.SetGloves(assetRef.Gloves);
        }

        SetText(assetName);
    }
}
