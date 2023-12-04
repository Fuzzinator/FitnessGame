using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class NewPlaylistEnvAssetSetter : MonoBehaviour, IEnvAssetScroller
{
    [SerializeField]
    protected TextMeshProUGUI _assetNameDisplayText;
    [SerializeField]
    protected EnhancedScroller _scroller;
    [SerializeField]
    protected EnvAssetScrollerController _controller;

    [SerializeField]
    protected EnvAssetRef _assetOverride;

    protected virtual void OnEnable()
    {
        _assetOverride = null;
        GetAndSetText();
        if (EnvironmentControlManager.Instance != null)
        {
            EnvironmentControlManager.Instance.availableReferencesUpdated.AddListener(GetAndSetText);
        }
        if (PlaylistManager.Instance != null)
        {
            PlaylistMaker.Instance.TargetEnvironmentIndexChanged.AddListener(UpdateFromEnvIndexChange);
        }
    }

    protected virtual void OnDisable()
    {
        if (EnvironmentControlManager.Instance != null)
        {
            EnvironmentControlManager.Instance.availableReferencesUpdated.RemoveListener(GetAndSetText);
        }
        if(PlaylistManager.Instance != null)
        {
            PlaylistMaker.Instance.TargetEnvironmentIndexChanged.RemoveListener(UpdateFromEnvIndexChange);
        }
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

    public abstract EnvAssetRef GetAssetRef(int assetIndex);

    public abstract int GetAvailableAssetCount();

    public abstract void SetAssetIndex(int index);

    protected abstract string GetAssetName();

    protected abstract string GetDefaultAssetName();

    protected abstract void UpdateFromEnvIndexChange(int index);

    protected virtual void GetAndSetText()
    {
        var assetName = GetAssetName();
        if(string.IsNullOrWhiteSpace(assetName))
        {
            assetName = GetDefaultAssetName();
        }
        SetText(assetName);
    }

    public virtual void SetText(string value)
    {
        _assetNameDisplayText.text = value;
    }
}
