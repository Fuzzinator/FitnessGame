using Cysharp.Text;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class TMProExtensions
{
    private static readonly char[] Blank = new char[0];
    public static void ClearText(this TMP_Text text)
    {
        text.SetCharArray(Blank);
    }

    public static void ClearText(this TMP_InputField text, bool notify = true)
    {
        text.textComponent.ClearText();
        text.placeholder.enabled = true;
        if (notify && text.onValueChanged != null)
            text.onValueChanged.Invoke(string.Empty);
    }

    public static void SetTextZeroAlloc(this TMP_Text textField, string text, bool notNested)
    {
        using (var sb = ZString.CreateStringBuilder(notNested))
        {
            sb.Append(text);
            textField.SetText(sb);
        }
    }

    public static void SetTextZeroAlloc(this TMP_Text textField, float value, bool notNested)
    {
        using (var sb = ZString.CreateStringBuilder(notNested))
        {
            sb.Append(value);
            textField.SetText(sb);
        }
    }
}
