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

                var description = VersionController.Instance.VersionDescriptions[i];
                switch (description.TargetPlatform)
                {
                    case TargetPlatform.All:
#if UNITY_ANDROID
                case TargetPlatform.Android:
#elif UNITY_STANDALONE_WIN
                    case TargetPlatform.PCVR:
#endif
                        break;
                    default:
                        continue;
                }

                sb.Append(description.VersionNumber);
                sb.AppendLine();
                sb.Append(description.Description);
                sb.AppendLine();
                sb.AppendLine();
            }

            var buffer = sb.AsArraySegment();
            _textField.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
        };
    }
}
