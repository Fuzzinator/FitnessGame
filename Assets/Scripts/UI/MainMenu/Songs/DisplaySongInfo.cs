using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static UnityEngine.XR.Hands.XRHandSubsystemDescriptor;

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
        private TextMeshProUGUI _songRating;

        [SerializeField]
        private SetAndShowSongOptions _songOptions;

        [SerializeField]
        private Button _deleteButton;

        [SerializeField]
        private Button _hideButton;

        [SerializeField]
        private TextMeshProUGUI _deleteButtonText;

        [SerializeField]
        private TextMeshProUGUI _hideButtonText;

        private SongInfo _currentSongInfo;

        private float _songSpeedModifier = 1f;

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
            if (_currentSongInfo != null && SongInfoFilesReader.Instance.filteredAvailableSongs.Exists((i) => i.fileLocation.Equals(_currentSongInfo.fileLocation, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }
            ClearDisplayedInfo();
        }

        private void OnDisable()
        {
            ClearDisplayedInfo();
        }
        public void SongSpeedModValueChanged(float value)
        {
            _songSpeedModifier = SongSliderToPlaylistSpeedMod(value);
            if(_currentSongInfo != null)
            {
                UpdateDisplayedInfo(_currentSongInfo);
            }
        }

        private void UpdateDisplayedInfo(SongInfo info)
        {
            _currentSongInfo = info;
            _songName.SetTextZeroAlloc(info.SongName, true);
            _songAuthor.SetTextZeroAlloc(info.SongAuthorName, true);
            _levelAuthor.SetTextZeroAlloc(info.LevelAuthorName, true);
            _songLength.SetTextZeroAlloc(info.GetReadableLength(_songSpeedModifier), true);
            _beatsPerMinute.SetTextZeroAlloc(info.BeatsPerMinute * _songSpeedModifier, true);

            if (info.SongScore < 0)
            {
                _songRating.SetTextZeroAlloc(string.Empty, true);
            }
            else
            {
                _songRating.SetTextZeroAlloc(info.SongScore * 100, true);
            }

            _songOptions.UpdateDifficultyOptions(info, info.DifficultySets);

            if (_deleteButton != null)
            {
                _deleteButton.gameObject.SetActive(info.isCustomSong);
                _deleteButtonText.gameObject.SetActive(info.isCustomSong);
            }

            if (_hideButton != null)
            {
                _hideButton.gameObject.SetActive(!info.isCustomSong);
                _hideButtonText.gameObject.SetActive(!info.isCustomSong);
            }
        }

        private void ClearDisplayedInfo()
        {
            _currentSongInfo = null;//new SongInfo();
            _songName.ClearText();
            _songAuthor.ClearText();
            _levelAuthor.ClearText();
            _songLength.ClearText();
            _songRating.ClearText();
            _beatsPerMinute.ClearText();
            _songOptions.HideOptions();

            if (_deleteButton != null)
            {
                _deleteButton.gameObject.SetActive(false);
                _deleteButtonText.gameObject.SetActive(false);
            }

            if (_hideButton != null)
            {
                _hideButton.gameObject.SetActive(false);
                _hideButtonText.gameObject.SetActive(false);
            }
        }

        public void ToggleSongPreview()
        {
            _songOptions.ToggleSongPreview();
        }

        private float SongSliderToPlaylistSpeedMod(float sliderValue)
        {
            switch ((int)sliderValue)
            {
                case 0:
                    return .75f;
                case 1:
                    return .875f;
                case 2:
                    return 1;
                case 3:
                    return 1.125f;
                case 4:
                    return 1.25f;
                default:
                    return 1;
            }
        }
    }
}