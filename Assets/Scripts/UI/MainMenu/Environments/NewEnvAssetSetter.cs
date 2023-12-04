using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class NewEnvAssetSetter : MonoBehaviour, IEnvAssetScroller
{
    [SerializeField]
    protected TextMeshProUGUI _assetNameDisplayText;
    [SerializeField]
    protected EnhancedScroller _scroller;
    [SerializeField]
    protected EnvAssetScrollerController _controller;
    [SerializeField]
    protected CustomEnvironmentCreator _envCreator;

    protected virtual void OnEnable()
    {
        GetAndSetText();
        if (EnvironmentControlManager.Instance != null)
        {
            EnvironmentControlManager.Instance.availableReferencesUpdated.AddListener(GetAndSetText);
        }
    }

    protected virtual void OnDisable()
    {
        if (EnvironmentControlManager.Instance != null)
        {
            EnvironmentControlManager.Instance.availableReferencesUpdated.RemoveListener(GetAndSetText);
        }
    }

    protected abstract void GetAndSetText();

    public virtual void DisableOptionsDisplay()
    {
        _scroller.gameObject.SetActive(false);
    }

    public virtual void EnableOptionsDisplay()
    {
        _controller.SetUp(this, _scroller);
        _scroller.gameObject.SetActive(true);
        _controller.Refresh();
    }

    public abstract EnvAssetRef GetAssetRef(int index);

    public abstract int GetAvailableAssetCount();

    public abstract void SetAssetIndex(int index);

    protected void SetText(string text)
    {
        _assetNameDisplayText.text = text;
    }

    public bool TrySetText(string text)
    {
        if(string.IsNullOrWhiteSpace(text))
        {
            return false;
        }
        SetText(text); 
        return true;
    }
}
