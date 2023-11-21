using Cysharp.Text;
using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class EnvironmentAssetSetter : MonoBehaviour
{
    [SerializeField]
    protected TextMeshProUGUI _assetNameDisplayText;
    [SerializeField]
    protected EnhancedScroller _scroller;

    protected virtual void OnEnable()
    {
        GetAndSetText();
        if (EnvironmentControlManager.Instance != null)
        {
            EnvironmentControlManager.Instance.availableReferencesUpdated.AddListener(GetAndSetText);
            EnvironmentControlManager.Instance.targetEnvironmentIndexChanged.AddListener(UpdateFromEnvIndexChange);
        }

        PlaylistManager.Instance.currentPlaylistUpdated.AddListener(UpdateFromPlaylist);
    }

    protected virtual void OnDisable()
    {
        if (EnvironmentControlManager.Instance != null)
        {
            EnvironmentControlManager.Instance.availableReferencesUpdated.RemoveListener(GetAndSetText);
            EnvironmentControlManager.Instance.targetEnvironmentIndexChanged.RemoveListener(UpdateFromEnvIndexChange);
        }

        PlaylistManager.Instance.currentPlaylistUpdated.RemoveListener(UpdateFromPlaylist);
    }

    public virtual void EnableOptionsDisplay()
    {
        _scroller.gameObject.SetActive(true);
    }

    protected virtual string GetAssetName(Playlist sourcePlaylist)
    {
        var assetName = GetPlaylistAssetName(sourcePlaylist);
        if (string.IsNullOrWhiteSpace(sourcePlaylist.TargetEnvTargetsName) && EnvironmentControlManager.Instance != null)
        {
            assetName = GetAssetFromEnvIndex(0).Name;
        }
        return assetName;
    }

    protected virtual Environment GetAssetFromEnvIndex(int index)
    {
        var environmentExists = EnvironmentControlManager.Instance.TryGetEnvRefAtIndex(index, out var env);
        if (environmentExists)
        {
            return env;
        }
        return new Environment();
    }

    public abstract string GetPlaylistAssetName(Playlist playlist);

    public abstract string GetEnvAssetName(Environment environment);

    protected abstract bool ShouldUpdateFromEnv();

    public abstract void SetAssetIndex(int index);

    protected abstract int GetAssetIndex();

    public virtual void SetText(string value)
    {
        using var sb = ZString.CreateStringBuilder(true);
        sb.AppendLine(value);
        _assetNameDisplayText.SetText(sb);
    }

    protected virtual void GetAndSetText()
    {
        var name = GetAssetName(PlaylistManager.Instance?.CurrentPlaylist);
        SetText(name);
    }

    protected virtual void UpdateFromPlaylist(Playlist playlist)
    {
        var assetName = GetAssetName(playlist);
        SetText(assetName);
    }

    protected virtual void UpdateFromEnvIndexChange(int index)
    {
        if(!ShouldUpdateFromEnv())
        {
            return;
        }
        var environment = GetAssetFromEnvIndex(index);
        var assetName = GetEnvAssetName(environment);
        SetText(assetName);
    }
}
