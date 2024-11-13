using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileSelectionController : MonoBehaviour
{
    public static ProfileSelectionController Instance { get; private set; }


    [SerializeField]
    private ProfileChoiceDisplay _displayObjectPrefab;

    [SerializeField]
    private List<ProfileChoiceDisplay> _activeChoices = new List<ProfileChoiceDisplay>();

    [SerializeField]
    private Transform _choiceParent;

    [SerializeField]
    private ProfileEditor _profileEditor;

    [SerializeField]
    private Animator _fogAnimator;

    [SerializeField]
    private GameObject _noProfileDisplay;

    [SerializeField]
    private Canvas _mainCanvas;

    private PoolManager _choicesPoolManager;
    private bool _fogActive = false;

    public ProfileEditor ProfileEditor => _profileEditor;

    private bool _initialized = false;

    private const string FogOn = "FogOn";
    private const string Transition = "Transition";

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnEnable()
    {
        DisplayProfileSelection();
    }

    private void OnDisable()
    {
        if (_fogActive)
        {
            _fogActive = false;
            _fogAnimator.SetBool(Transition, true);
            _fogAnimator.SetBool(FogOn, false);
        }

        ProfileManager.Instance.UnloadProfileSprites();
        HideProfileChoices();
        ProfileManager.Instance.profilesUpdated.RemoveListener(ShowAvailableProfiles);
    }

    public void DisplayProfileSelection()
    {
        _mainCanvas.enabled = true;

        if (ProfileManager.Instance.ActiveProfile == null)
        {
            _fogActive = true;
            _fogAnimator.SetBool(Transition, _initialized);
            _fogAnimator.SetBool(FogOn, true);
        }
        else
        {
            _fogActive = false;
            _fogAnimator.SetBool(Transition, false);
            _fogAnimator.SetBool(FogOn, false);
        }

        ProfileManager.Instance.GetAllProfileSprites();
        ShowAvailableProfiles();
        ProfileManager.Instance.profilesUpdated.AddListener(ShowAvailableProfiles);
        _initialized = true;
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
        while (_activeChoices.Count > 0)
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

    public void DisableCanvas()
    {
        _mainCanvas.enabled = false;
    }
}
