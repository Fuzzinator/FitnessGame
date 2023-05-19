using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayHitSpeeds : MonoBehaviour
{
    [SerializeField]
    private RectTransform _hitSpeedBar;
    [SerializeField]
    private RectTransform _averageSpeedIndicator;

    private void OnEnable()
    {
        ScoringAndHitStatsManager.Instance.UpdatedHitSpeed.AddListener(UpdateDisplay);
    }

    private void OnDisable()
    {
        ScoringAndHitStatsManager.Instance.UpdatedHitSpeed.RemoveListener(UpdateDisplay);
    }


    private void UpdateDisplay(float hitSpeed)
    {
        var hitSpeedNormalized = Normalize(hitSpeed);
        var averageSpeedNormalized = Normalize(ScoringAndHitStatsManager.Instance.AverageTotalHitSpeed);

        _hitSpeedBar.anchorMax = new Vector2(hitSpeedNormalized, 1);

        _averageSpeedIndicator.anchorMin = new Vector2(averageSpeedNormalized, 0);
        _averageSpeedIndicator.anchorMax = new Vector2(averageSpeedNormalized, 1);
    }

    private float Normalize(float hitSpeed)
    {
        return Mathf.Clamp01((hitSpeed-SettingsManager.DefaultMinHitSpeed) / (SettingsManager.DefaultMaxHitSpeed - SettingsManager.DefaultMinHitSpeed));
    }
}
