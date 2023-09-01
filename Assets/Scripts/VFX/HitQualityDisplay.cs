using Cysharp.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HitInfo;

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

    private const string SwingMore = "Swing More!";
    private const string WrongWay = "Wrong Way!";
    private const string TooSlow = "Too Slow!";
    private const string BadForm = "Bad Form!";
    #endregion

    public void Initialize()
    {
    }

    public void ReturnToPool()
    {
        _displayText.ClearText();
        gameObject.SetActive(false);
        MyPoolManager.ReturnToPool(this);
    }

    public void SetHitDisplay(HitInfo info)
    {
        var qualityName = info.QualityName switch
        {
            HitQualityName.Perfect => Perfect,
            HitQualityName.Great => Great,
            HitQualityName.Good => Good,
            HitQualityName.Okay => Okay,
            HitQualityName.Bad => Bad,
            _ => null
        };
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.Append(qualityName);
            _displayText.SetText(sb);
        }
    }

    public void SetMissedDisplay(ValidHit validHit)
    {
        var missName = string.Empty;
        if(!validHit.IsSwinging)
        {
            missName = SwingMore;
        }
        else if(!validHit.InHitAllowance)
        {
            missName = WrongWay;
        }
        else if(!validHit.FastEnough)
        {
            missName = TooSlow;
        }
        else if(!validHit.GoodForm)
        {
            missName = BadForm;
        }
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.Append(missName);
            _displayText.SetText(sb);
        }
    }

    public void UpdateScaleAndAlpha(float scale, float alpha)
    {
        transform.localScale = Vector3.one* scale;
        _displayText.alpha = alpha;
    }
}
