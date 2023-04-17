using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR.Core;

public class WatchVisualReference : MonoBehaviour
{
    [Header("Indicator settings")]
    public MeshRenderer indicator;
    public Material onlineMaterial;
    public Material offlineMaterial;

    [Header("Diplayer settings")]
    public SpriteRenderer onlineDisplay;
    public SpriteRenderer offlineDisplay;

    private void Update()
    {
        if (YURInterface.Instance.HasLogin)
        {
            SetOnline();
        }
        else
        {
            SetOffline();
        }
    }

    private void SetOnline()
    {
        if (indicator && onlineMaterial)
            indicator.material = onlineMaterial;

        onlineDisplay.enabled = true;
        offlineDisplay.enabled = false;
    }

    private void SetOffline()
    {
        if (indicator && offlineMaterial)
            indicator.material = offlineMaterial;

        onlineDisplay.enabled = false;
        offlineDisplay.enabled = true;
    }

}
