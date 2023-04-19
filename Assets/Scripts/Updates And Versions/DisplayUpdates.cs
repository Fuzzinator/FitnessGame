using Cysharp.Text;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayUpdates : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textField;

    [SerializeField]
    private UpdateDescriptionObject[] _textAssets;

    public UpdateDescriptionObject MostRecentUpdate => _textAssets?[^1];

    private void OnEnable()
    {
        SetText();
    }
    private void SetText()
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            for (var i = _textAssets.Length-1; i >=0; i--)
            {
                var asset = _textAssets[i];
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
