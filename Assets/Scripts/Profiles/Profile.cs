using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class Profile
{
    [field: SerializeField]
    public string ProfileName { get; private set; }
    
    [field: SerializeField]
    public string IconAddress { get; private set; }

    [field: SerializeField]
    public bool CustomIcon { get; private set; }

    [field: SerializeField]
    public string GUID { get; private set; }

    [field: SerializeField]
    public bool UseOnlineLeaderboards { get; private set; }

    private Sprite _sprite;

    public Profile(string profileName, string iconAddress, bool customIcon)
    {
        ProfileName = profileName;
        IconAddress = iconAddress;
        CustomIcon = customIcon;
        GUID = Guid.NewGuid().ToString();
    }

    public Profile()
    {
        
    }
    
    public void SetName(string newName)
    {
        ProfileName = newName;
    }

    public void SetIconAddress(string iconAddress, bool isCustom)
    {
        IconAddress = iconAddress;
        CustomIcon = isCustom;
        UpdateSprite().Forget();
    }

    public async UniTask<Sprite> GetSprite()
    {
        if (_sprite != null)
        {
            return _sprite;
        }

        await UpdateSprite();
        return _sprite;
    }

    private async UniTask UpdateSprite()
    {
        _sprite = await ProfileManager.LoadSprite(this);
    }
}
