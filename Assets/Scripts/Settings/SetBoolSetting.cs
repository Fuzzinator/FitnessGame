using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetBoolSetting : MonoBehaviour
{
    [SerializeField]
    private string _settingName;

    public void SetSetting(bool settingValue)
    {
        SettingsManager.SetSetting(_settingName, settingValue);
    }
}
