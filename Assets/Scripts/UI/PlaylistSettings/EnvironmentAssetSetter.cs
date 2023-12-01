using Cysharp.Text;
using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class EnvironmentAssetSetter : MonoBehaviour, IEnvAssetScroller
{
    [SerializeField]
    protected TextMeshProUGUI _assetNameDisplayText;
    [SerializeField]
    protected EnhancedScroller _scroller;
    [SerializeField]
    protected EnvAssetScrollerController _controller;

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
        _controller.SetUp(this, _scroller);
        _scroller.gameObject.SetActive(true);
        _controller.Refresh();
    }

    public virtual void DisableOptionsDisplay()
    {
        _scroller.gameObject.SetActive(false);
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

    public abstract int GetAvailableAssetCount();

    public abstract string GetPlaylistAssetName(Playlist playlist);

    public abstract string GetEnvAssetName(Environment environment);

    protected abstract bool ShouldUpdateFromEnv();

    public abstract void SetAssetIndex(int index);

    public abstract EnvAssetRef GetAssetRef(int index);
    protected abstract void TrySetAsset(Playlist playlist);

    protected abstract int GetAssetIndex();


    public virtual void SetText(string value)
    {
        _assetNameDisplayText.SetTextZeroAlloc(value, true);
    }

    protected virtual void GetAndSetText()
    {
        var name = GetAssetName(PlaylistManager.Instance?.CurrentPlaylist);
        SetText(name);
    }

    protected virtual void UpdateFromPlaylist(Playlist playlist)
    {
        var assetName = GetAssetName(playlist);
        TrySetAsset(playlist);

        if (string.IsNullOrWhiteSpace(assetName))
        {
            if(EnvironmentControlManager.Instance!= null)
            {

            }
            assetName = "Sci-Fi Arena";
        }
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
        if (environment.IsCustom && string.IsNullOrWhiteSpace(assetName))
        {
            assetName = "Sci-Fi Arena";
        }
        SetText(assetName);
    }
}
