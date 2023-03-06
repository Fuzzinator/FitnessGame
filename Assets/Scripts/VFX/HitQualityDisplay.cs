using Cysharp.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitQualityDisplay : MonoBehaviour, IPoolable
{
    [SerializeField]
    TMPro.TextMeshProUGUI _displayText;

    public PoolManager MyPoolManager { get; set; }
    public bool IsPooled { get; set; }

    private const float FootInMeters = 0.3048f;

    #region Const Strings
    private const string Bad = "Bad";
    private const string Okay = "Okay";
    private const string Good = "Good";
    private const string Great = "Great";
    private const string Perfect = "Perfect";
    #endregion

    public void Initialize()
    {
    }

    public void ReturnToPool()
    {
        _displayText.SetCharArray(null);
        gameObject.SetActive(false);
        MyPoolManager.ReturnToPool(this);
    }

    public void SetDisplay(HitInfo info)
    {
        var hitQuality = GetModifierRange(info);
        var qualityName = hitQuality switch
        {
            _ when hitQuality < .2f => Perfect,
            _ when hitQuality < .4f => Great,
            _ when hitQuality < .6f => Good,
            _ when hitQuality < .8f => Okay,
            _ when hitQuality >= .8f => Bad,
            _ => null
        };
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.Append(qualityName);
            _displayText.SetText(sb);
        }
    }

    public void UpdateScaleAndAlpha(float scale, float alpha)
    {
        transform.localScale = Vector3.one* scale;
        _displayText.alpha = alpha;
    }

    private float GetModifierRange(HitInfo info)
    {
        var distance = 1f - Mathf.Clamp(info.DistanceFromOptimalHit, 0, FootInMeters) * 3.333f;
        var impactValue = Mathf.Clamp(info.ImpactDotProduct, 0, 1);
        var directionValue = Mathf.Clamp(info.DirectionDotProduct, 0, 1);
        var final = 1f - ((distance + impactValue + directionValue) * .333f);
        //var magnitudeBonusValue = 1- Mathf.Clamp(info.HitSpeed, 0, 30) * .1f;
        return final;
    }
}
