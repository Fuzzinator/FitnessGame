using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Scrollers.Playlists
{
    public class SongInfoCellView : EnhancedScrollerCellView
    {
        [FormerlySerializedAs("_songName")] [SerializeField]
        private TextMeshProUGUI _songDetails;

        [SerializeField]
        private Button _button;

        private SongInfo _songInfo;
        private AvailableSongInfoScrollerController _controller;
        
        private const string SONGINFOFORMAT =
            "<align=center>{0}</style>\n<size=50%>{1}<line-indent=15%>{2}<line-indent=15%>{3}</size></align>";

        private void OnValidate()
        {
            if (_button == null)
            {
                TryGetComponent(out _button);
            }
        }

        public void SetData(SongInfo info, AvailableSongInfoScrollerController controller)
        {
            _songInfo = info;
            _controller = controller;
            var readableLength = info.ReadableLength;
            using (var sb = ZString.CreateStringBuilder(true))
            {
                sb.AppendFormat(SONGINFOFORMAT, 
                    info.SongName, 
                    info.SongAuthorName, 
                    info.LevelAuthorName,
                    readableLength);

                _songDetails.SetText(sb);
            }
        }

        public void SetActiveSongInfo()
        {
            _controller.SetActiveInfo(_songInfo);
        }
    }
}