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
        hasKey = SettingsManager.GetSetting(PLAYEDBEFORE, false);
        if (hasKey)
        {
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

        var mainMenu = MainMenuUIController.Instance;
        mainMenu.SetActivePage(_targetMenuPage);
    }
}