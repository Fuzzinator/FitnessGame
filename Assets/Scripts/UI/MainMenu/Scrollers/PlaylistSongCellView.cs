using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using GameModeManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Scrollers.Playlists
{
    public class PlaylistSongCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private TextMeshProUGUI _songName;

        [SerializeField]
        private TextMeshProUGUI _songDifficulty;

        [FormerlySerializedAs("_beatsPerMinute")] [SerializeField]
        private TextMeshProUGUI _songLength;

        [SerializeField]
        private Image _highlight;
        [SerializeField]
        private Image _invalidIndicator;

        public void SetData(PlaylistItem playlist)
        {
            _songName?.SetText(playlist.SongName);
            _songDifficulty?.SetText(playlist.Difficulty);
            _songLength?.SetText(playlist.TargetGameMode.GetDisplayName());
            SetInvalidIndicator(playlist).Forget();
        }
        
        private async UniTaskVoid SetInvalidIndicator(PlaylistItem item)
        {
            var isValid = await PlaylistValidator.IsValid(item);
            _invalidIndicator.gameObject.SetActive(!isValid);
        }

        public void SetHighlight(bool on)
        {
            _highlight.enabled = on;
        }
    }
}