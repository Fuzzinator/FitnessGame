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
        _profileName.SetText(profile.ProfileName);
        _profile = profile;
        _profileSelectionController = profileSelectionController;
        SetProfileImage().Forget();
    }

    public void SetSelectedProfile()
    {
        if (!SettingsManager.HasSetting("TargetSideSetting", true, _profile))
        {
            _profileSelectionController.StartEditProfile(_profile);
            _profileSelectionController.ProfileEditor.SetActivePage(2);
            return;
        }
        else if(!SettingsManager.HasSetting(SettingsManager.UseAdaptiveStrikeMode, true, _profile))
        {
            _profileSelectionController.StartEditProfile(_profile);
            _profileSelectionController.ProfileEditor.SetActivePage(3);
            return;
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
        _profileName.SetText(string.Empty);
    }

    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        MyPoolManager.ReturnToPool(this);
    }
}
