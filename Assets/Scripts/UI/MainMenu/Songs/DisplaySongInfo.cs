using System;
using System.Collections;
using System.Collections.Generic;
using GameModeManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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

        [SerializeField]
        private Button _deleteButton;

        private SongInfo _currentSongInfo;

        private GameMode _selectedGameMode;

        public void UpdateDisplayedInfo(SongInfo info)
        {
            _currentSongInfo = info;
            _songName.SetText(info.SongName);
            _songAuthor.SetText(info.SongAuthorName);
            _levelAuthor.SetText(info.LevelAuthorName);
            _songLength.SetText(info.ReadableLength);
            _difficultyScroller.UpdateDifficultyOptions(info, info.DifficultySets, _selectedGameMode);
            _deleteButton.gameObject.SetActive(info.isCustomSong);
        }

        public void ClearDisplayedInfo()
        {
            _currentSongInfo = new SongInfo();
            _songName.SetText(string.Empty);
            _songAuthor.SetText(string.Empty);
            _levelAuthor.SetText(string.Empty);
            _songLength.SetText(string.Empty);
            _difficultyScroller.UpdateDifficultyOptions(_currentSongInfo, Array.Empty<SongInfo.DifficultySet>(),
                GameMode.Normal);
            _deleteButton.gameObject.SetActive(false);
        }

        public void SetTargetGameMode(int gameMode)
        {
            _selectedGameMode = (GameMode) gameMode;
        }
    }
}