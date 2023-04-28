using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class TMProDisplayUpdator : MonoBehaviour
{
    [SerializeField]
    private string _prefix = string.Empty;

    [SerializeField]
    private string _suffix = string.Empty;
    
    [SerializeField]
    private TextMeshProUGUI _targetText;

    private const string FORMAT = "{0}{1}{2}";
    
    public void UpdateText(string value)
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.AppendFormat(FORMAT, _prefix, value, _suffix);

            var buffer = sb.AsArraySegment();
            _targetText.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
        }
    }

    public void UpdateText(ulong value)
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.AppendFormat(FORMAT, _prefix, value, _suffix);

            var buffer = sb.AsArraySegment();
            _targetText.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
        }
    }
    
    public void UpdateText(int value)
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.AppendFormat(FORMAT, _prefix, value, _suffix);

            var buffer = sb.AsArraySegment();
            _targetText.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
        }
    }

    public void UpdateText(TextHolder textHolder)
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.AppendFormat(FORMAT, _prefix, textHolder.Text, _suffix);

            var buffer = sb.AsArraySegment();
            _targetText.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
        }
    }

    public void UpdateText<T>(T value)
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.AppendFormat(FORMAT, _prefix, value, _suffix);

            var buffer = sb.AsArraySegment();
            _targetText.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
        }
    }
}
