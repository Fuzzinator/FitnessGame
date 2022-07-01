using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR;

public class SetTargetFrameRate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_ANDROID

        var headset = OVRPlugin.GetSystemHeadsetType();

        OVRPlugin.systemDisplayFrequency = headset != OVRPlugin.SystemHeadset.Oculus_Quest ? 90 : 72;

        //OVRPlugin.fixedFoveatedRenderingLevel = OVRPlugin.FixedFoveatedRenderingLevel.Medium;
#endif
    }
}