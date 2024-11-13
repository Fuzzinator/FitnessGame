using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Profiling;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance { get; private set; }

    private Profile _activeProfile;

    private List<Profile> _profiles = new List<Profile>();

    private Dictionary<Sprite, ProfileIconInfo> _spriteDictionary = new Dictionary<Sprite, ProfileIconInfo>();
    private List<Sprite> _sprites = new List<Sprite>();
    private List<AsyncOperationHandle> _spriteAssetHandles = new List<AsyncOperationHandle>();

    public UnityEvent activeProfileUpdated = new UnityEvent();
    public UnityEvent profilesUpdated = new UnityEvent();

    public Profile ActiveProfile => _activeProfile;
    public List<Profile> Profiles => _profiles;

    public List<Sprite> ProfileIcons => _sprites;
    public ES3Settings ProfileSettings { get; private set; }

    private CancellationToken _cancellationToken;

    private const string PROFILES = "UserProfiles";
    private const string PROFILESETTINGS = "Profiles/Settings";
    private const string PROFILEICONLABEL = "Profile Icon";
    private const string LOCALPROFILEICONSLOCATION = "Assets/Art/Textures/Profile Icons/";
    public const string DEFAULTICONADDRESS = "Icon_Account.Png";

#if UNITY_EDITOR
    private const string PROFILEICONSLOCATION = "/LocalCustomIcons/";
#else
    private const string PROFILEICONSLOCATION = "/Resources/Profile Icons/";
#endif

    private static string _profileIconsPath;

    public static string ProfileIconsPath
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_profileIconsPath))
            {
                _profileIconsPath = $"{AssetManager.DataPath}{PROFILEICONSLOCATION}";
            }

            return _profileIconsPath;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    private void OnDestroy()
    {
        if (ActiveProfile == null)
        {
            return;
        }
        SaveCachedProfileSettings(ActiveProfile);
    }

    public void ClearActiveProfile()
    {
        if (ActiveProfile != null)
        {
            SaveCachedProfileSettings(ActiveProfile);
        }
        _activeProfile = null;
    }

    public void DeleteProfile(Profile profile)
    {
        _profiles.Remove(profile);
        profilesUpdated?.Invoke();
        if (_activeProfile == profile)
        {
            _activeProfile = null;
            activeProfileUpdated?.Invoke();
        }

        SaveProfiles();

        if (profile == null)
        {
            return;
        }

        DeleteSaveFile(profile);
    }

    public Profile CreateProfile(string profileName, string iconAddress, bool customImage)
    {
        var newProfile = new Profile(profileName, iconAddress, customImage);
        SettingsManager.SetSetting("Creation Date", DateTime.Now.ToShortTimeString(), true, newProfile);

        AddProfile(newProfile);
        return newProfile;
    }

    public void TryGetProfiles()
    {
        var profiles = SettingsManager.GetSetting<List<Profile>>(PROFILES, null, false);
        SetProfiles(profiles);
    }

    public void SetProfiles(List<Profile> profiles)
    {
        if (profiles == null)
        {
            return;
        }

        _profiles = profiles;
        _profiles.Sort((x, y) => string.Compare(x.ProfileName, y.ProfileName));
        profilesUpdated?.Invoke();

        if (_profiles.Count == 1)
        {
            SetActiveProfile(_profiles[0]);
        }
    }

    public void AddProfile(Profile profile)
    {
        _profiles ??= new List<Profile>();

        _profiles.Add(profile);
        _profiles.Sort((x, y) => string.Compare(x.ProfileName, y.ProfileName));
        profilesUpdated?.Invoke();

#if !UNITY_EDITOR_WIN && !UNITY_STANDALONE_WIN
        SaveCachedProfileSettings(profile);
#endif

        SaveProfiles();
    }

    public void SetActiveProfile(Profile profile)
    {
        SettingsManager.ClearCachedValues();
        _activeProfile = profile;
        ProfileSettings = GetNewProfileSettings(profile);

        activeProfileUpdated?.Invoke();
    }

    public void ActiveProfileUpdated()
    {
        activeProfileUpdated?.Invoke();
    }

    public void UpdateProfile(Profile profile, string profileName, string address, bool isCustomIcon)
    {
        if (!string.Equals(profile.ProfileName, profileName))
        {
            RenameSaveFile(profile, profileName, address, isCustomIcon);
            profile.SetName(profileName);
            if (profile.UseOnlineLeaderboards)
            {
                AzureSqlManager.Instance.UpdateDisplayName(profileName);
            }
        }

        profile.SetIconAddress(address, isCustomIcon);

        SaveCachedProfileSettings(profile);
        profilesUpdated?.Invoke();
        if (profile == _activeProfile)
        {
            activeProfileUpdated?.Invoke();
        }

        SaveProfiles();
    }

    private void SaveProfiles()
    {
        SettingsManager.SetSetting(PROFILES, _profiles, false);
    }

    public ProfileIconInfo TryGetInfoFromSprite(Sprite sprite)
    {
        if (sprite == null || !_spriteDictionary.ContainsKey(sprite))
        {
            return new ProfileIconInfo(string.Empty, false);
        }

        return _spriteDictionary[sprite];
    }

    public Sprite TryGetSpriteFromInfo(ProfileIconInfo info)
    {
        if (string.IsNullOrWhiteSpace(info.Address))
        {
            info.Address = DEFAULTICONADDRESS;
        }

        if (!_spriteDictionary.ContainsValue(info))
        {
            return null;
        }

        foreach (var keySet in _spriteDictionary)
        {
            if (info == keySet.Value)
            {
                return keySet.Key;
            }
        }

        return null;
    }

    public void GetAllProfileSprites()
    {
        GetBuiltInProfileSprites().Forget();
    }

    private async UniTaskVoid GetBuiltInProfileSprites()
    {
        var resourceHandle = Addressables.LoadResourceLocationsAsync("Profile Icon", typeof(Sprite));

        var spriteLocations = await resourceHandle;
        foreach (var location in spriteLocations)
        {
            var spriteLoadHandle = Addressables.LoadAssetAsync<Sprite>(location);
            if (!_spriteAssetHandles.Contains(spriteLoadHandle))
            {
                _spriteAssetHandles.Add(spriteLoadHandle);
            }

            var sprite = await spriteLoadHandle;
            ;
            if (sprite == null)
            {
                continue;
            }

            var address = location.PrimaryKey;
            address = address.Substring(address.LastIndexOf('/') + 1);
            var info = new ProfileIconInfo(address, false);
            _spriteDictionary[sprite] = info;

            if (!_sprites.Contains(sprite))
            {
                _sprites.Add(sprite);
            }
        }

        Addressables.Release(resourceHandle);
    }

    public void UnloadProfileSprites()
    {
        _spriteDictionary.Clear();
        foreach (var handle in _spriteAssetHandles)
        {
            Addressables.Release(handle);
        }

        _spriteAssetHandles.Clear();
    }

    public static async UniTask<Sprite> LoadSprite(Profile profile)
    {
        if (profile.CustomIcon)
        {
            return await LoadCustomSprite(profile);
        }
        else
        {
            return await LoadBuiltInSprite(profile);
        }
    }

    private static async UniTask<Sprite> LoadBuiltInSprite(Profile profile)
    {
        try
        {
            var filename = profile.IconAddress;
            if (string.IsNullOrWhiteSpace(filename))
            {
                filename = DEFAULTICONADDRESS;
            }

            var request = Addressables.LoadAssetAsync<Sprite>($"{LOCALPROFILEICONSLOCATION}{filename}");
            await request.ToUniTask(cancellationToken: Instance._cancellationToken);
            if (Instance._cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            if (request.Result == null)
            {
                Debug.LogError($"Failed to load local resource file for {profile.ProfileName}");
            }

            return request.Result;
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
            return null;
        }
    }

    private static async UniTask<Sprite> LoadCustomSprite(Profile profile)
    {
        if (!await PermissionsRequester.Instance.HasReadAndWritePermissions())
        {
            return null;
        }

        var path = $"{ProfileIconsPath}{profile.IconAddress}";

        var texture = await AssetManager.LoadImageFromPath(path, Instance._cancellationToken);
        if (texture == null)
        {
            return null;
        }

        var textureSize = new Vector2(texture.width, texture.height);
        var spriteRect = new Rect(Vector2.zero, textureSize);
        var sprite = Sprite.Create(texture, spriteRect, Vector2.zero);
        return sprite;
    }

    private static void RenameSaveFile(Profile profile, string profileName, string address, bool isCustomIcon)
    {
        var originalPath = new ES3Settings($"{PROFILESETTINGS}{profile.ProfileName}.{profile.GUID}.dat");
        var newPath = new ES3Settings($"{PROFILESETTINGS}{profileName}.{profile.GUID}.dat");
        ES3.RenameFile(originalPath, newPath);
    }

    private static void DeleteSaveFile(Profile profile)
    {
        var settings = GetProfileSettings(profile);
        if (settings == null)
        {
            return;
        }
        ES3.DeleteFile(settings);
    }

    public static ES3Settings GetNewProfileSettings(Profile profile)
    {
        var settingName = $"{PROFILESETTINGS}{profile.ProfileName}.{profile.GUID}.dat";
        try
        {
            ES3.CacheFile(settingName);
        }
        catch (Exception ex)
        {
            if (ex is FormatException)
            {
                return null;
            }

            Debug.LogError($"Cant get:{settingName}--{ex.Message}--{ex.StackTrace}");
            return null;
        }

        return new ES3Settings(settingName, ES3.Location.Cache);
    }

    public static ES3Settings GetProfileSettings(Profile profile)
    {
        if (profile == Instance.ActiveProfile)
        {
            return Instance.ProfileSettings;
        }
        else
        {
            return GetNewProfileSettings(profile);
        }
    }

    public void CleanUpCorruptedProfile(Profile overrideProfile)
    {
        if (overrideProfile != null)
        {
            DeleteProfile(overrideProfile);
        }
        else if (ActiveProfile != null)
        {
            DeleteProfile(ActiveProfile);
        }

        var currentScene = SceneManager.GetActiveScene();
        if (currentScene.name != "Main Menu")
        {
            GameStateManager.Instance.SetState(GameState.Playing);
            PlaylistManager.Instance.FullReset();

            if (TransitionController.Instance != null)
            {
                TransitionController.Instance.RequestTransition();
            }
        }
        else if (ProfileSelectionController.Instance != null)
        {
            ProfileSelectionController.Instance.DisplayProfileSelection();
        }
    }

    public void SaveCachedProfileSettings(Profile profile)
    {
        var settingName = $"{PROFILESETTINGS}{profile.ProfileName}.{profile.GUID}.dat";
        ES3.StoreCachedFile(settingName);
    }

    public struct ProfileIconInfo
    {
        public string Address { get; set; }
        public bool IsCustom { get; set; }

        public ProfileIconInfo(string address, bool isCustom)
        {
            Address = address;
            IsCustom = isCustom;
        }

        public static bool operator ==(ProfileIconInfo item1, ProfileIconInfo item2)
        {
            return string.Equals(item1.Address, item2.Address) && item1.IsCustom == item2.IsCustom;
        }

        public static bool operator !=(ProfileIconInfo item1, ProfileIconInfo item2)
        {
            return !(item1 == item2);
        }

        public override bool Equals(object obj)
        {
            if (obj is not ProfileIconInfo iconInfo)
            {
                return false;
            }
            return this == iconInfo;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Address, IsCustom);
        }
    }
}