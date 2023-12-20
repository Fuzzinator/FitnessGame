using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Scrollers.Playlists
{
    public class DisplaySongInfo : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup _canvasGroup;
        
        [SerializeField]
        private TextMeshProUGUI _songName;

        [SerializeField]
        private TextMeshProUGUI _songAuthor;

        [SerializeField]
        private TextMeshProUGUI _levelAuthor;

        [SerializeField]
        private TextMeshProUGUI _songLength;

        [SerializeField]
        private TextMeshProUGUI _beatsPerMinute;

        [SerializeField]
        private SetAndShowSongOptions _songOptions;

        [SerializeField]
        private Button _deleteButton;
        [SerializeField]
        private TextMeshProUGUI _deleteButtonText;

        private SongInfo _currentSongInfo;

        public void RequestDisplay(SongInfo info)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                gameObject.SetActive(true);
            }

            UpdateDisplayedInfo(info);
        }

        public void RequestCloseDisplay()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = true;
                gameObject.SetActive(false);
            }
        }

        public void CheckIfShouldClear()
        {
            if(_currentSongInfo != null && SongInfoFilesReader.Instance.availableSongs.Exists((i) => i.fileLocation.Equals(_currentSongInfo.fileLocation, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }
            ClearDisplayedInfo();
        }
        
        private void OnDisable()
        {
            ClearDisplayedInfo();
        }

        private void UpdateDisplayedInfo(SongInfo info)
        {
            _currentSongInfo = info;
            _songName.SetTextZeroAlloc(info.SongName, true);
            _songAuthor.SetTextZeroAlloc(info.SongAuthorName, true);
            _levelAuthor.SetTextZeroAlloc(info.LevelAuthorName, true);
            _songLength.SetTextZeroAlloc(info.ReadableLength, true);
            _beatsPerMinute.SetTextZeroAlloc(info.BeatsPerMinute, true);
            _songOptions.UpdateDifficultyOptions(info, info.DifficultySets);
            _deleteButton.gameObject.SetActive(info.isCustomSong);
            _deleteButtonText.gameObject.SetActive(info.isCustomSong);
        }

        private void ClearDisplayedInfo()
        {
            _currentSongInfo = new SongInfo();
            _songName.ClearText();
            _songAuthor.ClearText();
            _levelAuthor.ClearText();
            _songLength.ClearText();
            _beatsPerMinute.ClearText();
            _songOptions.HideOptions();
            _deleteButton.gameObject.SetActive(false);
            _deleteButtonText.gameObject.SetActive(false);
        }

        public void ToggleSongPreview()
        {
            _songOptions.ToggleSongPreview();
        }
    }
}