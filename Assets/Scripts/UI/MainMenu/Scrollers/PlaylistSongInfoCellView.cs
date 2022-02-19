using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using GameModeManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Scrollers.Playlists
{
    public class PlaylistSongInfoCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private TextMeshProUGUI _songName;

        [SerializeField]
        private TextMeshProUGUI _songGameMode;
        
        [SerializeField]
        private TextMeshProUGUI _songDifficulty;

        [SerializeField] private Image _invalidIndicator;
        public void SetData(PlaylistItem item)
        {
            _songName.SetText(item.SongName);
            _songGameMode.SetText(item.TargetGameMode.GetDisplayName());
            _songDifficulty.SetText(item.Difficulty);
            SetInvalidIndicator(item).Forget();
        }

        private async UniTaskVoid SetInvalidIndicator(PlaylistItem item)
        {
            var isValid = await PlaylistValidator.IsValid(item);
            _invalidIndicator.enabled = !isValid;
        }
    }
}