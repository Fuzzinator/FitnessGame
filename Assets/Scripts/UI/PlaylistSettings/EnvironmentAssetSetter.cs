using Cysharp.Text;
using Cysharp.Threading.Tasks;
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
    [SerializeField]
    protected bool _ignorePlaylists = false;


    protected virtual void OnEnable()
    {
        WaitAndUpdateDisplay().Forget();
        if (EnvironmentControlManager.Instance != null)
        {
            EnvironmentControlManager.Instance.availableReferencesUpdated.AddListener(GetAndSetText);
            EnvironmentControlManager.Instance.targetEnvironmentIndexChanged.AddListener(UpdateFromEnvIndexChange);
        }
        if(_ignorePlaylists)
        {
            return;
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

        if (_ignorePlaylists)
        {
            return;
        }
        PlaylistManager.Instance.currentPlaylistUpdated.RemoveListener(UpdateFromPlaylist);
    }

    private async UniTaskVoid WaitAndUpdateDisplay()
    {
        if(this == null)
        {
            return;
        }
        ResetOverrides();
        SaveLog($"{this.GetType()}: Trying to Get And Set Text");
        GetAndSetText();
        SaveLog($"{this.GetType()}: Successfully Got And Set Text");

        if (_ignorePlaylists)
        {
            return;
        }

        if (PlaylistManager.Instance?.CurrentPlaylist != null)
        {
            UpdateFromPlaylist(PlaylistManager.Instance.CurrentPlaylist);
        }

        await UniTask.Delay(System.TimeSpan.FromSeconds(1));
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

    public abstract EnvAssetReference GetAssetRef(int index);
    protected abstract void TrySetAsset(Playlist playlist);

    protected abstract int GetAssetIndex();

    protected abstract void ResetOverrides();


    public virtual void SetText(string value)
    {
        _assetNameDisplayText.text = value;
    }

    protected virtual void GetAndSetText()
    {
        var name = GetAssetName(PlaylistManager.Instance?.CurrentPlaylist);
        SetText(name);
    }

    protected virtual void UpdateFromPlaylist(Playlist playlist)
    {
        SaveLog($"{this.GetType()}: Trying to get asset name");

        var assetName = GetAssetName(playlist);

        SaveLog($"{this.GetType()}: Got asset name");
        TrySetAsset(playlist);

        SaveLog($"{this.GetType()}: Set Asset");
        if (string.IsNullOrWhiteSpace(assetName))
        {
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


    protected void SaveLog(string message)
    {
        //ErrorReporter.Instance.LogMessage(message, string.Empty, LogType.Log);
    }
}
