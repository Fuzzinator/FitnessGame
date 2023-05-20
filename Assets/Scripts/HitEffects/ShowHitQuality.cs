using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHitQuality : MonoBehaviour, IValidHit, IBadHit
{
    public void TriggerBadHitEffect(HitInfo info, ValidHit validHit)
    {
        if (VFXManager.Instance == null)
        {
            return;
        }

        var qualityDisplay = GetHitQualityDisplay();

        qualityDisplay.SetMissedDisplay(validHit);
        qualityDisplay.gameObject.SetActive(true);
    }

    public void TriggerHitEffect(HitInfo info)
    {
        if(VFXManager.Instance == null)
        {
            return;
        }

        var qualityDisplay = GetHitQualityDisplay();

        qualityDisplay.SetHitDisplay(info);
        qualityDisplay.gameObject.SetActive(true);
    }

    private HitQualityDisplay GetHitQualityDisplay()
    {
        var qualityDisplay = HitQualityDisplayManager.GetHitQualityDisplay();
        var thisTransform = transform;
        var displayTransform = qualityDisplay.transform;
        displayTransform.rotation = thisTransform.rotation;
        displayTransform.position = thisTransform.position;
        return qualityDisplay;
    }
}
