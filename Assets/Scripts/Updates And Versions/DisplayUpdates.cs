using Cysharp.Text;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayUpdates : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textField;


    private void OnEnable()
    {
        SetText();
    }
    private void SetText()
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            var length = VersionController.Instance.VersionDescriptions.Length;
            for (var i = length-1; i >=0; i--)
            {
                var asset = VersionController.Instance.VersionDescriptions[i];
                sb.Append(asset.VersionNumber);
                sb.AppendLine();
                sb.Append(asset.Description);
                sb.AppendLine();
                sb.AppendLine();
            }

            var buffer = sb.AsArraySegment();
            _textField.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
        };
    }
}
