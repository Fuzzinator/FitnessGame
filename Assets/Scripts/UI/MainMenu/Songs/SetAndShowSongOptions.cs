using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using GameModeManagement;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class SetAndShowSongOptions : MonoBehaviour
{
    [SerializeField]
    private ToggleGroup _gameTypeToggleGroup;

    [SerializeField]
    private Toggle[] _gameTypeToggles;

    [SerializeField]
    private ToggleGroup _difficultyToggleGroup;
    [SerializeField]
    private Toggle[] _typeDifficultyToggles;

    [SerializeField]
    private DisplaySongRecords _songRecordsDisplay;
    public string SelectedDifficulty => _selectedDifficulty;
    public GameMode SelectedGameMode => _activeDifficultySet.MapGameMode;
    
    private SongInfo _songInfo;
    private SongInfo.DifficultySet[] _difficultySets;
    private SongInfo.DifficultySet _activeDifficultySet;
    private string _selectedDifficulty;

    public void UpdateDifficultyOptions(SongInfo songInfo, SongInfo.DifficultySet[] difficultySets)
    {
        _songInfo = songInfo;
        _difficultySets = difficultySets;
        gameObject.SetActive(true);
        UpdateAvailableGameModes();
    }

    public void HideOptions()
    {
        gameObject.SetActive(false);
    }
    
    private void UpdateAvailableGameModes()
    {
        _gameTypeToggleGroup.gameObject.SetActive(false);
        foreach (var toggle in _gameTypeToggles)
        {
            toggle.gameObject.SetActive(false);
        }

        var lowest = 10;
        for (var i = 0; i < _difficultySets.Length; i++)
        {
            if (_difficultySets[i].MapGameMode == GameMode.LightShow)
            {
                continue;
            }
            var toggleID = (int)_difficultySets[i].MapGameMode-1;
            
            if (_difficultySets[i].MapGameMode == GameMode.Lawless)
            {
                toggleID--;
            }

            if (toggleID < lowest)
            {
                lowest = toggleID;
            }
            _gameTypeToggles[toggleID].gameObject.SetActive(true);
        }

        _gameTypeToggleGroup.gameObject.SetActive(true);
        _gameTypeToggles[lowest].SetIsOnWithoutNotify(true);
        SetSelectedType(_gameTypeToggles[lowest]);
    }
    
    private void UpdateAvailableDifficulties()
    {
        _difficultyToggleGroup.gameObject.SetActive(false);

        foreach (var toggle in _typeDifficultyToggles)
        {
            toggle.gameObject.SetActive(false);
        }
        
        var lowest = 10;
        for (var i = 0; i < _activeDifficultySet.DifficultyInfos.Length; i++)
        {
            var difficulty = (int)_activeDifficultySet.DifficultyInfos[i].DifficultyAsEnum -1;

            if (difficulty< lowest)
            {
                lowest = difficulty;
            }
            _typeDifficultyToggles[difficulty].gameObject.SetActive(true);
        }

        _difficultyToggleGroup.gameObject.SetActive(true);
        _typeDifficultyToggles[lowest].SetIsOnWithoutNotify(true);
        SetSelectedDifficulty(_typeDifficultyToggles[lowest]);
    }
    
    public void SetSelectedType(Toggle toggle)
    {
        if (!toggle.isOn)
        {
            return;
        }
        
        var toggleID = GetToggleID(toggle, _gameTypeToggles);

        if (toggleID < 0)
        {
            Debug.LogError("Song does not have matching toggle");
            return;
        }

        var gameModeID = -1;
        for (var i = 0; i < _difficultySets.Length; i++)
        {
            var gameMode = (int)_difficultySets[i].MapGameMode-1;
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
        
        var toggleID = GetToggleID(toggle, _typeDifficultyToggles);

        if (toggleID < 0)
        {
            Debug.LogError("Song does not have matching toggle");
            return;
        }
        var dificultyID = -1;
        for (var i = 0; i < _activeDifficultySet.DifficultyInfos.Length; i++)
        {
            var difficulty = (int)_activeDifficultySet.DifficultyInfos[i].DifficultyAsEnum -1;
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
        _songRecordsDisplay?.RefreshDisplay();
    }

    private int GetToggleID(Toggle toggle, Toggle[] togglesArray)
    {
        var toggleID = -1;
        for (var i = 0; i < togglesArray.Length; i++)
        {
            if (togglesArray[i] == toggle)
            {
                toggleID = i;
                break;
            }
        }

        return toggleID;
    }
    
    public void AddSelectedPlaylistItem()
    {
        if (PlaylistMaker.Instance != null)
        {
            var playlistItem = PlaylistMaker.GetPlaylistItem(_songInfo, _selectedDifficulty, _activeDifficultySet.MapGameMode);
            PlaylistMaker.Instance.AddPlaylistItem(playlistItem);
        }
    }

    public void PlayIndividualSong()
    {
        if (PlaylistManager.Instance != null)
        {
            var playlistItem = new PlaylistItem(_songInfo, _selectedDifficulty, _activeDifficultySet.MapGameMode);
            PlaylistManager.Instance.SetTempSongPlaylist(playlistItem);
        }
    }
}
