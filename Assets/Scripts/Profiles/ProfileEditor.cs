using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileEditor : UIMenuController
{
    [SerializeField]
    private TMP_InputField _inputField;

    [SerializeField]
    private Image _profileImage;

    [SerializeField]
    private Button _deleteButton;

    [SerializeField]
    private EnhancedScroller _profileImagesScroller;

    [SerializeField]
    private MonoBehaviour[] _settings;

    private string _iconAddress = ProfileManager.DEFAULTICONADDRESS;

    private Profile _profile = null;
    private bool _isCustomIcon = false;

    public Profile ActiveProfile => _profile;

    protected override void OnEnable()
    {
        SetActivePage(0);
    }

    public void StartEditProfile(Profile profile)
    {
        _profile = profile;
        _inputField.text = profile.ProfileName;
        SetIconAddress(profile.IconAddress);
        SetImage();
        SetIsCustomIcon(profile.CustomIcon);
        _deleteButton.gameObject.SetActive(true);
        OpenEditProfile();
    }

    public void StartCreateProfile()
    {
        _profile = null;
        _deleteButton.gameObject.SetActive(false);
        OpenEditProfile();
    }

    private void OpenEditProfile()
    {
        gameObject.SetActive(true);
    }

    public void CompleteEditProfile()
    {
        if (_profile == null)
        {
            _profile = ProfileManager.Instance.CreateProfile(_inputField.text, _iconAddress, _isCustomIcon);            
        }
        else
        {
            if (string.IsNullOrWhiteSpace(_iconAddress))
            {
                _iconAddress = ProfileManager.DEFAULTICONADDRESS;
            }

            ProfileManager.Instance.UpdateProfile(_profile, _inputField.text, _iconAddress, _isCustomIcon);
        }

        foreach (ISaver setting in _settings)
        {
            if (setting == null || !setting.SaveRequested)
            {
                continue;
            }
            setting.Save(_profile);
        }

        _profile = null;

        ResetCreateNewProfile();
    }

    public void DeleteProfile()
    {
        if (_profile != null)
        {
            ProfileManager.Instance.DeleteProfile(_profile);
        }
        ResetCreateNewProfile();
    }

    public void ResetCreateNewProfile()
    {
        _inputField.text = string.Empty;
        _iconAddress = ProfileManager.DEFAULTICONADDRESS;
        SetIsCustomIcon(false);
        SetImage();
        gameObject.SetActive(false);
    }

    public void NewIconSelected(ProfileManager.ProfileIconInfo info, Sprite sprite)
    {
        _iconAddress = info.Address;
        _isCustomIcon = info.IsCustom;
        SetImage(sprite);
        _profileImagesScroller.gameObject.SetActive(false);
    }

    public void SetIsCustomIcon(bool isCustom)
    {
        _isCustomIcon = isCustom;
    }

    public void SetIconAddress(string iconAddress)
    {
        _iconAddress = iconAddress;
    }

    private void SetImage()
    {
        var info = new ProfileManager.ProfileIconInfo(_iconAddress, _isCustomIcon);
        _profileImage.sprite = ProfileManager.Instance.TryGetSpriteFromInfo(info);
    }

    private void SetImage(Sprite sprite)
    {
        _profileImage.sprite = sprite;
    }
}