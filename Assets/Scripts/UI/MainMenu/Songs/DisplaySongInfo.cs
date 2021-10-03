using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

        [SerializeField]
        private TextMeshProUGUI _beatsPerMinute;

        [SerializeField]
        private SongDifficultyScrollerController _difficultyScroller;

        private SongInfo _currentSongInfo;

        public void UpdateDisplayedInfo(SongInfo info)
        {
            _currentSongInfo = info;
            _songName.SetText(info.SongName);
            _songAuthor.SetText(info.SongAuthorName);
            _levelAuthor.SetText(info.LevelAuthorName);
            _beatsPerMinute.SetText(info.BeatsPerMinute.ToString("00"));
            _difficultyScroller.UpdateDifficultyOptions(info.DifficultySets[0]);
        }
    }
}