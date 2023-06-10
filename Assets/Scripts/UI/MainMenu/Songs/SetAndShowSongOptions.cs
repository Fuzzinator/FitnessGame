using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using GameModeManagement;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using static DifficultyInfo;

public class SetAndShowSongOptions : MonoBehaviour
{
    [SerializeField]
    private ToggleGroup _gameTypeToggleGroup;

    [SerializeField]
    private Toggle[] _gameTypeToggles;

    [SerializeField]
    private TextMeshProUGUI[] _gameTypeTexts;

    [SerializeField]
    private ToggleGroup _difficultyToggleGroup;
    [SerializeField]
    private Toggle[] _typeDifficultyToggles;

    [SerializeField]
    private TextMeshProUGUI[] _typeDifficultyTexts;

    [SerializeField]
    private DisplaySongRecords _songRecordsDisplay;

    [SerializeField]
    private SetTargetForwardFoot _forwardFootSetter;

    [SerializeField]
    private Button _playButton;
    [SerializeField]
    private TextMeshProUGUI _playButtonText;

    public string SelectedDifficulty => _selectedDifficulty;
    public DifficultyInfo.DifficultyEnum DifficultyAsEnum => _difficultyEnum;
    public GameMode SelectedGameMode => _activeDifficultySet.MapGameMode;

    private SongInfo _songInfo;
    private SongInfo.DifficultySet[] _difficultySets;
    private SongInfo.DifficultySet _activeDifficultySet;
    private string _selectedDifficulty;
    private DifficultyInfo.DifficultyEnum _difficultyEnum;

    public void UpdateDifficultyOptions(SongInfo songInfo, SongInfo.DifficultySet[] difficultySets)
    {
        _songInfo = songInfo;
        _difficultySets = difficultySets;
        //gameObject.SetActive(true);
        _gameTypeToggleGroup.gameObject.SetActive(true);
        _difficultyToggleGroup.gameObject.SetActive(true);
        _playButton.gameObject.SetActive(true);
        _playButtonText.gameObject.SetActive(true);
        _gameTypeTexts[0].transform.parent.gameObject.SetActive(true);
        _typeDifficultyTexts[0].transform.parent.gameObject.SetActive(true);

        UpdateAvailableGameModes();
    }

    public void HideOptions()
    {
        //gameObject.SetActive(false);
        _gameTypeToggleGroup.gameObject.SetActive(false);
        _difficultyToggleGroup.gameObject.SetActive(false);
        _playButton.gameObject.SetActive(false);
        _playButtonText.gameObject.SetActive(false);
        _gameTypeTexts[0].transform.parent.gameObject.SetActive(false);
        _typeDifficultyTexts[0].transform.parent.gameObject.SetActive(false);
    }

    private void UpdateAvailableGameModes()
    {
        gameObject.SetActive(false);
        _gameTypeTexts[0].transform.parent.gameObject.SetActive(false);
        _typeDifficultyTexts[0].transform.parent.gameObject.SetActive(false);

        for (var i = 0; i < _gameTypeToggles.Length; i++)
        {
            var toggle = _gameTypeToggles[i];
            var text = _gameTypeTexts[i];

            toggle.gameObject.SetActive(false);
            text.gameObject.SetActive(false);
        }

        var lowest = 10;
        for (var i = 0; i < _difficultySets.Length; i++)
        {
            if (_difficultySets[i].MapGameMode is GameMode.LightShow or GameMode.Lawless)
            {
                continue;
            }
            var toggleID = (int)_difficultySets[i].MapGameMode - 1;

            /*if (_difficultySets[i].MapGameMode == GameMode.Lawless)
            {
                toggleID--;
            }*/

            if (toggleID < lowest)
            {
                lowest = toggleID;
            }
            _gameTypeToggles[toggleID].gameObject.SetActive(true);
            _gameTypeTexts[toggleID].gameObject.SetActive(true);
        }

        gameObject.SetActive(true);
        _gameTypeTexts[0].transform.parent.gameObject.SetActive(true);
        _typeDifficultyTexts[0].transform.parent.gameObject.SetActive(true);

        _gameTypeToggles[lowest].SetIsOnWithoutNotify(true);
        SetSelectedType(_gameTypeToggles[lowest]);
    }

    private void UpdateAvailableDifficulties()
    {
        gameObject.SetActive(false);
        _gameTypeTexts[0].transform.parent.gameObject.SetActive(false);
        _typeDifficultyTexts[0].transform.parent.gameObject.SetActive(false);

        for (var i = 0; i < _typeDifficultyToggles.Length; i++)
        {
            var toggle = _typeDifficultyToggles[i];
            var text = _typeDifficultyTexts[i];
            toggle.gameObject.SetActive(false);
            text.gameObject.SetActive(false);
        }

        var lowest = 10;
        var hasCurrentID = -1;
        for (var i = 0; i < _activeDifficultySet.DifficultyInfos.Length; i++)
        {
            var difficulty = (int)_activeDifficultySet.DifficultyInfos[i].DifficultyAsEnum - 1;
            if (difficulty < 0)
            {
                continue;
            }

            if (difficulty < lowest)
            {
                lowest = difficulty;
            }

            if (_activeDifficultySet.DifficultyInfos[i].DifficultyAsEnum == _difficultyEnum)
            {
                hasCurrentID = difficulty;
            }

            _typeDifficultyToggles[difficulty].gameObject.SetActive(true);
            _typeDifficultyTexts[difficulty].gameObject.SetActive(true);
        }

        gameObject.SetActive(true);
        _gameTypeTexts[0].transform.parent.gameObject.SetActive(true);
        _typeDifficultyTexts[0].transform.parent.gameObject.SetActive(true);

        if (hasCurrentID > 0)
        {
            _typeDifficultyToggles[hasCurrentID].SetIsOnWithoutNotify(true);
            SetSelectedDifficulty(_typeDifficultyToggles[hasCurrentID]);
        }
        else
        {
            _typeDifficultyToggles[lowest].SetIsOnWithoutNotify(true);
            SetSelectedDifficulty(_typeDifficultyToggles[lowest]);
        }
    }

    public void SetSelectedType(Toggle toggle)
    {
        if (!toggle.isOn)
        {
            return;
        }

        var toggleID = _gameTypeToggles.GetToggleID(toggle);

        if (toggleID < 0)
        {
            Debug.LogError("Song does not have matching toggle");
            return;
        }

        var gameModeID = -1;
        for (var i = 0; i < _difficultySets.Length; i++)
        {
            var gameMode = (int)_difficultySets[i].MapGameMode - 1;
            if (_difficultySets[i].MapGameMode == GameMode.Lawless)
            {
                gameMode--;
            }

            if (toggleID == gameMode)
            {
                gameModeID = i;
                break;
            }
        }

        if (gameModeID < 0)
        {
            Debug.LogError("Song does not have matching Game Mode");
            return;
        }

        _activeDifficultySet = _difficultySets[gameModeID];
        UpdateAvailableDifficulties();
    }

    public void SetSelectedDifficulty(Toggle toggle)
    {
        if (!toggle.isOn)
        {
            return;
        }

        var toggleID = _typeDifficultyToggles.GetToggleID(toggle);

        if (toggleID < 0)
        {
            Debug.LogError("Song does not have matching toggle");
            return;
        }
        var dificultyID = -1;
        for (var i = 0; i < _activeDifficultySet.DifficultyInfos.Length; i++)
        {
            var difficulty = (int)_activeDifficultySet.DifficultyInfos[i].DifficultyAsEnum - 1;
            if (difficulty == toggleID)
            {
                dificultyID = i;
                break;
            }
        }

        if (dificultyID < -0)
        {
            Debug.LogError("Song does not have matching difficulty");
            return;
        }

        _selectedDifficulty = _activeDifficultySet.DifficultyInfos[dificultyID].Difficulty;
        _difficultyEnum = _activeDifficultySet.DifficultyInfos[dificultyID].DifficultyAsEnum;
        _songRecordsDisplay?.RefreshDisplay();
    }

    public void AddSelectedPlaylistItem()
    {
        if (PlaylistMaker.Instance != null)
        {
            var playlistItem = PlaylistMaker.GetPlaylistItem(_songInfo, _selectedDifficulty, _difficultyEnum, _activeDifficultySet.MapGameMode);
            PlaylistMaker.Instance.AddPlaylistItem(playlistItem);
        }
    }

    public void PlayIndividualSong()
    {
        if (PlaylistManager.Instance != null)
        {
            var playlistItem = new PlaylistItem(_songInfo, _selectedDifficulty, _difficultyEnum, _activeDifficultySet.MapGameMode);
            PlaylistManager.Instance.SetTempSongPlaylist(playlistItem, _forwardFootSetter.TargetHitSideType);
        }
    }
}
