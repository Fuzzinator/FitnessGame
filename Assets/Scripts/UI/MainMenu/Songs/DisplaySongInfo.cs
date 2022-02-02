using System.Collections;
using System.Collections.Generic;
using GameModeManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Scrollers.Playlists
{
    public class DisplaySongInfo : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _songName;

        [SerializeField]
        private TextMeshProUGUI _songAuthor;

        [SerializeField]
        private TextMeshProUGUI _levelAuthor;

        [SerializeField] [FormerlySerializedAs("_beatsPerMinute")]
        private TextMeshProUGUI _songLength;

        [SerializeField]
        private SongDifficultyScrollerController _difficultyScroller;

        private SongInfo _currentSongInfo;

        private GameMode _selectedGameMode;

        public void UpdateDisplayedInfo(SongInfo info)
        {
            _currentSongInfo = info;
            _songName.SetText(info.SongName);
            _songAuthor.SetText(info.SongAuthorName);
            _levelAuthor.SetText(info.LevelAuthorName);
            _songLength.SetText(info.ReadableLength);
            _difficultyScroller.UpdateDifficultyOptions(info, info.DifficultySets , _selectedGameMode);
        }

        public void SetTargetGameMode(int gameMode)
        {
            _selectedGameMode = (GameMode) gameMode;
        }
    }
}