using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileChoiceDisplay : MonoBehaviour, IPoolable
{
    [SerializeField]
    private Image _profileIcon;

    [SerializeField]
    private TextMeshProUGUI _profileName;

    private Profile _profile;
    private ProfileSelectionController _profileSelectionController;
    public PoolManager MyPoolManager { get; set; }
    public bool IsPooled { get; set; }

    public void SetData(Profile profile, ProfileSelectionController profileSelectionController)
    {
        _profileName.SetTextZeroAlloc(profile.ProfileName, true);
        _profile = profile;
        _profileSelectionController = profileSelectionController;
        SetProfileImage().Forget();
    }

    public void SetSelectedProfile()
    {
        try
        {
            var settings = ProfileManager.GetProfileSettings(_profile);
            if(settings == null)
            {

                HandleFormatException(_profile);
                _profileSelectionController.DisableCanvas();
                return;
            }

            if (!SettingsManager.HasSetting("TargetSideSetting", true, _profile))
            {
                _profileSelectionController.StartEditProfile(_profile);
                _profileSelectionController.ProfileEditor.SetActivePage(2);
                return;
            }
            else if (!SettingsManager.HasSetting(SettingsManager.UseAdaptiveStrikeMode, true, _profile))
            {
                _profileSelectionController.StartEditProfile(_profile);
                _profileSelectionController.ProfileEditor.SetActivePage(3);
                return;
            }
        }
        catch(Exception ex)
        {

            if (ex is FormatException)
            {
                return;
            }
            Debug.LogError($"Cant set:TargetSideSetting or {SettingsManager.UseAdaptiveStrikeMode}--{ex.Message}--{ex.StackTrace}");
        }

        ProfileManager.Instance.SetActiveProfile(_profile);
    }

    public void EditProfile()
    {
        _profileSelectionController.StartEditProfile(_profile);
    }

    private async UniTaskVoid SetProfileImage()
    {
        _profileIcon.sprite = await _profile.GetSprite();
    }

    public void Initialize()
    {
        _profileIcon.sprite = null;
        _profileName.ClearText();
    }

    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        MyPoolManager.ReturnToPool(this);
    }
    private void HandleFormatException(Profile overrideProfile)
    {
        var visuals = new Notification.NotificationVisuals("It appears your profile has been corrupted. Selecting \"Confirm\" will delete your current profile and you will be prompted to make a new one.", "Profile Corrupted", "Confirm", "Cancel");
        NotificationManager.RequestNotification(visuals, () => ProfileManager.Instance.CleanUpCorruptedProfile(overrideProfile), _profileSelectionController.DisplayProfileSelection);
    }
}
