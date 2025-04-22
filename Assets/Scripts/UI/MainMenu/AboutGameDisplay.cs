using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using TMPro;
using UnityEngine;

public class AboutGameDisplay : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textField;

    [SerializeField, TextArea]
    private string _information;

    private const string NEWLINE = "\n\n";
    
    private void OnEnable()
    {
        SetText();
    }

    private void SetText()
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            
            sb.Append(_information);

            foreach (var songInfo in SongInfoFilesReader.Instance.AvailableSongs)
            {
                if (songInfo.isCustomSong)
                {
                    continue;
                }
                sb.Append(NEWLINE);
                sb.Append(songInfo.Attribution);
            }

            var buffer = sb.AsArraySegment();
            _textField.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
        };
    }
}
