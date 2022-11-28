using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
public class SettingsProfileCtrl : MonoBehaviour
{
    [SerializeField]
    private MultiColorButton _profileIconButton;
    [SerializeField]
    private Image _profileIcon;

    [SerializeField]
    private int _profilePageIndex;

    private void OnEnable()
    {
        if (ProfileManager.Instance.ActiveProfile != null)
        {
            SetSprite().Forget();
        }

        _profileIconButton.interactable = MainMenuUIController.Instance != null;
    }

    private async UniTaskVoid SetSprite()
    {
        _profileIcon.sprite = await ProfileManager.Instance.ActiveProfile.GetSprite();
    }

    public void GoToProfileSelection()
    {
        MainMenuUIController.Instance.SetActivePage(_profilePageIndex);
    }
}

    
}