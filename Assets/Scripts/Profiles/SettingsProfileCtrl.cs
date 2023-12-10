using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SettingsProfileCtrl : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _profileNameDisplay;
        [SerializeField]
        private MultiColorButton _profileIconButton;
        [SerializeField]
        private Image _profileIcon;

        [SerializeField]
        private int _profilePageIndex;

        private void OnEnable()
        {
            if (ProfileManager.Instance != null)
            {
                if (_profileNameDisplay != null)
                {
                    ProfileManager.Instance.activeProfileUpdated.AddListener(SetProfileText);
                    SetProfileText();
                }
                if (ProfileManager.Instance.ActiveProfile != null)
                {
                    SetSprite().Forget();
                }
            }

            _profileIconButton.interactable = MainMenuUIController.Instance != null;
        }

        private void OnDisable()
        {
            if (_profileNameDisplay != null)
            {
                ProfileManager.Instance.activeProfileUpdated.RemoveListener(SetProfileText);
            }
        }

        private async UniTaskVoid SetSprite()
        {
            _profileIcon.sprite = await ProfileManager.Instance.ActiveProfile.GetSprite();
        }

        public void GoToProfileSelection()
        {
            ProfileManager.Instance.ClearActiveProfile();
            MainMenuUIController.Instance.SetActivePage(_profilePageIndex);
        }

        private void SetProfileText()
        {
            if (ProfileManager.Instance.ActiveProfile != null)
            {
                var greeting = $"Welcome {ProfileManager.Instance.ActiveProfile.ProfileName}";
                _profileNameDisplay.text = greeting;
            }
            else
            {
                _profileNameDisplay.text = null;
            }
        }
    }


}