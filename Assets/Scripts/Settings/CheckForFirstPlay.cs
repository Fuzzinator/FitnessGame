using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckForFirstPlay : MonoBehaviour
{
    [SerializeField]
    private int _targetMenuPage;

    private const string PLAYEDBEFORE = "HasPlayerPlayedBefore";
    private bool hasKey;

    private void Start()
    {
        ProfileManager.Instance.activeProfileUpdated.AddListener(RunCheck);
    }

    private void OnDestroy()
    {
        ProfileManager.Instance.activeProfileUpdated.RemoveListener(RunCheck);
    }

    private void RunCheck()
    {
        if (ProfileManager.Instance.ActiveProfile == null)
        {
            return;
        }

        hasKey = SettingsManager.GetSetting(PLAYEDBEFORE, false);
        if (hasKey)
        {
            GoToMainMenu();
            return;
        }

        ShowTutorialRequest();

        SettingsManager.SetSetting(PLAYEDBEFORE, true);
    }

    private void ShowTutorialRequest()
    {
        if (MainMenuUIController.Instance == null)
        {
            return;
        }

        MainMenuUIController.Instance.SetActivePage(_targetMenuPage);
    }

    private void GoToMainMenu()
    {
        if (MainMenuUIController.Instance == null)
        {
            return;
        }

        MainMenuUIController.Instance.SetActivePage(0);
    }
}