using System;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

using System;
using UnityEngine;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using Microsoft.Win32;
#endif

public class RuntimeChecker : MonoBehaviour
{
    void Start()
    {
        string currentRuntime = System.Environment.GetEnvironmentVariable("XR_RUNTIME_JSON");
        NotificationManager.RequestNotification(new Notification.NotificationVisuals(currentRuntime, "Current XR Runtime"));
    }

    void CheckOpenXRRuntimeEnvironment()
    {
        string xrRuntimeJson = System.Environment.GetEnvironmentVariable("XR_RUNTIME_JSON");

        if (!string.IsNullOrEmpty(xrRuntimeJson))
        {
            Debug.Log("XR_RUNTIME_JSON environment variable is set to: " + xrRuntimeJson);
            if (xrRuntimeJson.Contains("oculus"))
            {
                Debug.Log("Oculus OpenXR runtime is active.");
            }
            else if (xrRuntimeJson.Contains("steamvr"))
            {
                Debug.Log("SteamVR OpenXR runtime is active.");
            }
            else
            {
                Debug.Log("Unknown OpenXR runtime.");
            }
        }
        else
        {
            Debug.Log("XR_RUNTIME_JSON environment variable is not set.");
        }
    }
}
