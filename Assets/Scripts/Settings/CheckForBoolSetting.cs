using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckForBoolSetting : MonoBehaviour
{
    [SerializeField]
    private string _settingName;

    [SerializeField]
    private UnityEvent _settingTrue = new UnityEvent();
    
    [SerializeField]
    private UnityEvent _settingFalse = new UnityEvent();

    private void OnEnable()
    {
        ProfileManager.Instance.activeProfileUpdated.AddListener(CheckSetting);
        CheckSetting();
    }

    private void OnDisable()
    {
        ProfileManager.Instance.activeProfileUpdated.RemoveListener(CheckSetting);
    }

    private void CheckSetting()
    {
        var agreed = SettingsManager.GetSetting(_settingName, false);
        if (agreed)
        {
            _settingTrue?.Invoke();
        }
        else
        {
            _settingFalse?.Invoke();
        }
    }
    
}
