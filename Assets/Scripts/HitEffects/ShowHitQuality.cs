using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHitQuality : MonoBehaviour, IValidHit
{
    public void TriggerHitEffect(HitInfo info)
    {
        if(VFXManager.Instance == null)
        {
            return;
        }

        var qualityDisplay = HitQualityDisplayManager.GetHitQualityDisplay();
        var thisTransform = transform;
        var displayTransform = qualityDisplay.transform;
        displayTransform.rotation = thisTransform.rotation;
        displayTransform.position= thisTransform.position;

        qualityDisplay.SetDisplay(info);
        qualityDisplay.gameObject.SetActive(true);
    }
}
