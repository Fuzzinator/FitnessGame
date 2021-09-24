using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SetPlayerProportions : MonoBehaviour
{
    [SerializeField]
    private Transform _head;


    private const string RESETHEADSET = "Reset Headset";
    // Start is called before the first frame update
    void Start()
    {
        GlobalSettings.UserHeight = _head.position.y;
        PlayerPrefs.Save();
    }

    private void OnEnable()
    {
        InputManager.Instance.MainInput[RESETHEADSET].performed += ResetHeadset;
    }

    private void OnDisable()
    {
        InputManager.Instance.MainInput[RESETHEADSET].performed -= ResetHeadset;
    }

    private void ResetHeadset(InputAction.CallbackContext context)
    {
        GlobalSettings.UserHeight = _head.position.y;
    }
}
