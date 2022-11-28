using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileSelectionController : MonoBehaviour
{
    [SerializeField]
    private ProfileChoiceDisplay _displayObjectPrefab;

    [SerializeField]
    private List<ProfileChoiceDisplay> _activeChoices = new List<ProfileChoiceDisplay>();

    [SerializeField]
    private Transform _choiceParent;
    
    [SerializeField]
    private ProfileEditor _profileEditor;

    [SerializeField]
    private string _enableEffectName;
    
    [SerializeField]
    private string _disableEffectName;
    
    [SerializeField]
    private MaterialValueLerper _valueLerper;

    [SerializeField]
    private GameObject _noProfileDisplay;
    
    private PoolManager _choicesPoolManager;
    
    private void OnEnable()
    {
        _valueLerper.TriggerValueChange(_enableEffectName);
        
        ProfileManager.Instance.GetAllProfileSprites();
        ShowAvailableProfiles();
        ProfileManager.Instance.profilesUpdated.AddListener(ShowAvailableProfiles);
    }

    private void OnDisable()
    {
        _valueLerper.TriggerValueChange(_disableEffectName);
        
        ProfileManager.Instance.UnloadProfileSprites();
        HideProfileChoices();
        ProfileManager.Instance.profilesUpdated.RemoveListener(ShowAvailableProfiles);
    }

    public void ShowAvailableProfiles()
    {
        _choicesPoolManager ??= new PoolManager(_displayObjectPrefab, _choiceParent, 5);
        HideProfileChoices();
        
        foreach (var profile in ProfileManager.Instance.Profiles)
        {
            var poolable = _choicesPoolManager.GetNewPoolable();
            var choice = poolable as ProfileChoiceDisplay;
            if (choice == null)
            {
                poolable.ReturnToPool();
                Debug.LogError("Wrong object type returned from pool manager.");
                continue;
            }
            choice.SetData(profile, this);
            choice.gameObject.SetActive(true);
            _activeChoices.Add(choice);
        }

        var showNoProfiles = ProfileManager.Instance.Profiles.Count == 0;
        _noProfileDisplay.SetActive(showNoProfiles);
    }

    private void HideProfileChoices()
    {
        while (_activeChoices.Count>0)
        {
            _activeChoices[0].ReturnToPool();
            _activeChoices.RemoveAt(0);
        }
    }
    
    public void StartEditProfile(Profile profile)
    {
        _profileEditor.StartEditProfile(profile);
    }

    public void StartCreateProfile()
    {
        _profileEditor.StartCreateProfile();
    }
}
