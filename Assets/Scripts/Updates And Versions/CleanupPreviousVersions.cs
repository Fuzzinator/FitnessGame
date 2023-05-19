using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanupPreviousVersions : MonoBehaviour
{
    [SerializeField]
    private VersionAndCleanup[] _versionsToCleanUp = Array.Empty<VersionAndCleanup>();

    private const string Version = "CURRENT_VERSION";

    private void CheckVersions()
    {
        var previousVersion = SettingsManager.GetSetting<string>(Version, null);

        if(previousVersion == null)
        {
            return;
        }

        foreach(var version in _versionsToCleanUp)
        {
            if(string.Equals(version.Version, previousVersion))
            {
                foreach (var setting in version.SettingsToCleanup)
                {
                    SettingsManager.DeleteSetting(setting);
                }
                return;
            }
        }
    }

    [Serializable]
    private class VersionAndCleanup
    {
        [field: SerializeField]
        public string Version { get; private set; }
        [field: SerializeField]
        public string[] SettingsToCleanup { get; private set; }
    }
}
