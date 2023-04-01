using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuStateTracker : MonoBehaviour
{
    public static MainMenuStateTracker Instance { get; private set; }

    [field:SerializeField]
    public int ActivePage { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }
    }

    private void Start()
    {
        MainMenuUIController.OnMenuPageChange.AddListener(MenuPageChanged);
        ActivePage = -1;
    }

    private void MenuPageChanged(int page)
    {
        ActivePage = page;
    }
}
