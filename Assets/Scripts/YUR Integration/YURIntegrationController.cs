using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR.Core;

public sealed class YURIntegrationController : BaseGameStateListener
{
    [SerializeField]
    private YURWatch _yurWatch;

    private const string USEYUR = "EnableYUR";

    private void Awake()
    {
        if (GameManager.Instance != null && !GameManager.Instance.VRMode)
        {
            Destroy(gameObject);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        if (!SettingsManager.TrySubscribeToCachedBool(USEYUR, SetWatchState))
        {
            SettingsManager.CachedBoolSettingsChanged.AddListener(CheckIfShouldUpdate);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        SettingsManager.CachedBoolSettingsChanged.RemoveListener(CheckIfShouldUpdate);
    }

    private void CheckIfShouldUpdate(string settingName, bool value)
    {
        if (settingName == USEYUR)
        {
            if(SettingsManager.TrySubscribeToCachedBool(USEYUR, SetWatchState))
            {
                SettingsManager.CachedBoolSettingsChanged.RemoveListener(CheckIfShouldUpdate);
            }
        }
    }

    private void SetWatchState(bool useYUR)
    {
        _yurWatch.gameObject.SetActive(useYUR);
    }

    protected override void GameStateListener(GameState oldState, GameState newState)
    {
        var useYUR = SettingsManager.GetCachedBool(USEYUR, true);
        switch (newState)
        {
            case GameState.Entry:
                break;
            case GameState.Playing:
            case GameState.PreparingToPlay:
                SetWatchState(useYUR && YURWatch.IsConnected());
                break;
            case GameState.InMainMenu:
            case GameState.Paused:
            case GameState.Unfocused:
                SetWatchState(useYUR);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }
}
