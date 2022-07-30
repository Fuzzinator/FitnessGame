using System.Collections;
using System.Collections.Generic;
using BeatSaverSharp.Models;
using Cysharp.Text;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Scrollers.BeatsaverIntegraton
{
    public class BeatSaverSongCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private TextMeshProUGUI _songDetails;

        private Beatmap _beatmap;
        private BeatSaverSongsScrollerController _controller;
        
        private const string SONGINFOFORMAT =
            "<align=center>{0}</style>\n<size=50%><b>Song Author:</b> {1}<line-indent=10%><b>Level Author:</b> {2}<line-indent=10%><b>Song Score:</b> {3}</size></align>";
        
        public void SetData(Beatmap item, BeatSaverSongsScrollerController controller)
        {
            using (var sb = ZString.CreateStringBuilder(true))
            {
                sb.AppendFormat(SONGINFOFORMAT, 
                    item.Metadata.SongName, 
                    item.Metadata.SongAuthorName, 
                    item.Metadata.LevelAuthorName,
                    item.Stats.Score*100);

                _songDetails.SetText(sb);
            }
            
            _beatmap = item;
            _controller = controller;
        }

        public void Selected()
        {
            _controller.SetActiveBeatmap(_beatmap);
        }
    }
}