using Cysharp.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISliderStanceSwapRate : UISliderFloatSetting
{
    private const string Format = "{0} Seconds";
    public override void OnChangeSlider(float value)
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            value *= 60;

            sb.AppendFormat(Format, Mathf.RoundToInt(value));

            _currentText.SetText(sb);
        }
    }
}
