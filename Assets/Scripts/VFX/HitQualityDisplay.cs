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
        var hitQuality = 1 - info.HitQuality;
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
}
