using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileChecker : MonoBehaviour
{
    [SerializeField]
    private MainMenuUIController _menuController;

    [SerializeField]
    private int _targetPage;
    void Start()
    {
        if (ProfileManager.Instance.ActiveProfile != null)
        {
            return;
        }
        ProfileManager.Instance.TryGetProfiles();

        //TODO: Animate welcoming message to show active profile has been selected.
        if (ProfileManager.Instance.ActiveProfile != null)
        {
            return;
        }

        _menuController.SetActivePage(_targetPage);
    }
}
