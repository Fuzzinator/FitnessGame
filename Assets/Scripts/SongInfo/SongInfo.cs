using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SongInfo
{
    #region Const Strings

    private const int MINUTE = 60;

    #endregion

    public float BeatsPerMinute => _beatsPerMinute;

    public string SongName => _songName;
    public string SongFilename => _songFilename;

    public string SongAuthorName => _songAuthorName;

    public string LevelAuthorName => _levelAuthorName;

    public float SongLength
    {
        get { return _songLength; }
        set { _songLength = value; }
    }

    public float SongStartDelay => _songTimeOffset;

    public float LengthInMinutes => _songLength / MINUTE;

    public string ReadableLength
    {
        get
        {
            var minutes = Mathf.Floor(SongLength / MINUTE);
            var seconds = Mathf.Floor(SongLength % MINUTE);
            return ($"{minutes}:{seconds:00}");
        }
    }

    public DifficultySet[] DifficultySets => _difficultyBeatmapSets;

    [SerializeField]
    private string _songName;

    [SerializeField]
    private string _version;

    [SerializeField]
    private string _songSubName;

    [SerializeField]
    private string _songAuthorName;

    [SerializeField]
    private string _levelAuthorName;

    [SerializeField]
    private float _beatsPerMinute;

    [SerializeField]
    private float _songTimeOffset;

    [SerializeField]
    private string _songFilename;

    public string fileLocation;

    [NonSerialized]
    public bool isCustomSong;

    [SerializeField]
    private float _songLength;

    [SerializeField]
    private DifficultySet[] _difficultyBeatmapSets;

    public DifficultyInfo TryGetActiveDifficultySet(string difficulty)
    {
        for (var i = 0; i < _difficultyBeatmapSets.Length; i++)
        {
            var difficulties = _difficultyBeatmapSets[i];
            for (var j = 0; j < difficulties.DifficultyInfos.Length; j++)
            {
                var info = difficulties.DifficultyInfos[j];
                if (info.Difficulty == difficulty)
                {
                    return info;
                }
            }
        }

        return new DifficultyInfo();
    }

    [Serializable]
    public struct DifficultySet
    {
        private const string AUTONAME = "Auto-";
        private const string EASY = "Easy";
        private const string NORMAL = "Normal";
        private const string HARD = "Hard";
        private const string EXPERT = "Expert";
        private const string EXPERTPLUS = "ExpertPlus";

        [SerializeField]
        private DifficultyInfo[] _difficultyBeatmaps;

        public DifficultyInfo[] DifficultyInfos => _difficultyBeatmaps;

        public void TryCreateMissingDifficulties()
        {
            var hardestSet = new DifficultyInfo();
            var hasEasy = false;
            var hasNormal = false;
            var hasHard = false;
            var hasExpert = false;

            foreach (var difficulty in _difficultyBeatmaps)
            {
                switch (true) // determine which difficulties this set has
                {
                    case var b when difficulty.DifficultyRank <= DifficultyInfo.EASY:
                        hasEasy = true;
                        break;
                    case var b when difficulty.DifficultyRank <= DifficultyInfo.NORMAL:
                        hasNormal = true;
                        break;
                    case var b when difficulty.DifficultyRank <= DifficultyInfo.HARD:
                        hasHard = true;
                        break;
                    case var b when difficulty.DifficultyRank <= DifficultyInfo.EXPERT:
                        hasExpert = true;
                        break;
                }

                if (difficulty.DifficultyRank > hardestSet.DifficultyRank)
                {
                    hardestSet = difficulty;
                }
            }

            if (hasEasy && hasNormal && hasHard && hasExpert)
            {
                return;
            }

            var allDifficulties = new List<DifficultyInfo>(_difficultyBeatmaps);

            if (!hasEasy)
            {
                var easy = hardestSet;
                easy.SetDifficulty($"{AUTONAME}{EASY}", DifficultyInfo.EASY,
                    hardestSet.DifficultyRank > DifficultyInfo.EASY);
                allDifficulties.Add(easy);
            }

            if (!hasNormal)
            {
                var normal = hardestSet;
                normal.SetDifficulty($"{AUTONAME}{NORMAL}", DifficultyInfo.NORMAL,
                    hardestSet.DifficultyRank > DifficultyInfo.NORMAL);
                allDifficulties.Add(normal);
            }

            if (!hasHard)
            {
                var hard = hardestSet;
                hard.SetDifficulty($"{AUTONAME}{HARD}", DifficultyInfo.HARD,
                    hardestSet.DifficultyRank > DifficultyInfo.HARD);
                allDifficulties.Add(hard);
            }

            if (!hasExpert)
            {
                var expert = hardestSet;
                expert.SetDifficulty($"{AUTONAME}{EXPERT}", DifficultyInfo.EXPERT,
                    hardestSet.DifficultyRank > DifficultyInfo.EXPERT);
                allDifficulties.Add(expert);
            }

            allDifficulties.Sort((x, y) => x.DifficultyRank.CompareTo(y.DifficultyRank));
            _difficultyBeatmaps = allDifficulties.ToArray();
        }

        public void RemoveExpertPlus()
        {
            if (_difficultyBeatmaps.Length - 1 > -1 &&
                _difficultyBeatmaps[_difficultyBeatmaps.Length - 1].Difficulty == EXPERTPLUS)
            {
                var newArray = new DifficultyInfo[_difficultyBeatmaps.Length - 1];
                for (var i = 0; i < newArray.Length; i++)
                {
                    newArray[i] = _difficultyBeatmaps[i];
                }

                _difficultyBeatmaps = newArray;
            }
        }
    }


    public enum SortingMethod
    {
        None = 0,
        SongName = 1,
        InverseSongName = 2,
        AuthorName = 3,
        InverseAuthorName = 4,
        SongLength = 5,
        InverseSongLength = 6
    }
}