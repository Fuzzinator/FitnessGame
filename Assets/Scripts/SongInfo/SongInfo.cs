using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using GameModeManagement;
using Unity.Burst;
using UnityEngine;

[Serializable]
public class SongInfo
{
    #region Const Strings

    private const int MINUTE = 60;
    private const string DIVIDER = ":";

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
            using (var sb = ZString.CreateStringBuilder(true))
            {
                //sb.AppendFormat(LENGTHFORMAT, minutes, seconds);

                if (minutes < 10)
                {
                    sb.Append(0);
                }

                sb.Append(minutes);
                sb.Append(DIVIDER);
                if (seconds < 10)
                {
                    sb.Append(0);
                }

                sb.Append(seconds);

                //var buffer = sb.AsArraySegment();
                return sb.ToString();
            }
        }
    }

    public string Genre => _genre;

    public string Attribution => _attribution;

    public string ImageFilename => _coverImageFilename;

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
    private float _previewStartTime;

    [SerializeField]
    private float _previewDuration;

    [SerializeField]
    private string _songFilename;

    [SerializeField]
    private string _coverImageFilename;

    public string fileLocation;

    [NonSerialized]
    public bool isCustomSong;

    [SerializeField]
    private float _songLength;

    [SerializeField]
    private string _genre;

    [SerializeField]
    private string _attribution;

    [SerializeField]
    private DifficultySet[] _difficultyBeatmapSets;

    private Sprite _songArt;

    public DifficultyInfo TryGetActiveDifficultyInfo(string difficulty, GameMode gameMode)
    {
        var difficulties = GetBeatMapSet(gameMode);
        for (var j = 0; j < difficulties.DifficultyInfos.Length; j++)
        {
            var info = difficulties.DifficultyInfos[j];
            if (info.Difficulty == difficulty)
            {
                return info;
            }
        }

        return new DifficultyInfo();
    }

    public DifficultyInfo TryGetActiveDifficultyInfo(DifficultyInfo.DifficultyEnum difficulty, GameMode gameMode)
    {
        var difficulties = GetBeatMapSet(gameMode);
        for (var j = 0; j < difficulties.DifficultyInfos.Length; j++)
        {
            var info = difficulties.DifficultyInfos[j];
            if (info.DifficultyAsEnum == difficulty)
            {
                return info;
            }
        }

        return new DifficultyInfo();
    }

    public DifficultySet GetBeatMapSet(GameMode mode)
    {
        var setName = mode.GetDifficultySetName();
        if (string.IsNullOrWhiteSpace(setName))
        {
            Debug.LogError("Difficulty Not Set\n?");
            return _difficultyBeatmapSets.Length > 0 ? _difficultyBeatmapSets[0] : new DifficultySet();
        }

        foreach (var beatMapSet in _difficultyBeatmapSets)
        {
            if (beatMapSet.BeatMapName != null)
            {
                if (beatMapSet.MapGameMode == mode ||
                    beatMapSet.BeatMapName.Equals(setName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return beatMapSet;
                }
            }
        }

        Debug.LogError("No BeatMap found, returning beatMap 0");

        return _difficultyBeatmapSets.Length > 0 ? _difficultyBeatmapSets[0] : new DifficultySet();
    }

    public async UniTask<bool> UpdateDifficultySets(CancellationToken token)
    {
        var madeChange = false;
        var difficultySet = new DifficultySet();
        var has90RotationSet = false;
        var has360RotationSet = false;
        var createdRotationSet = false;
        var rotation90Set = new DifficultySet();
        var rotation360Set = new DifficultySet();

        /*List<DifficultySet> difficultySets = null;
        foreach (var set in _difficultyBeatmapSets)
        {
            if (set.DifficultyInfos != null)
            {
                continue;
            }

            madeChange = true;
            
            difficultySets ??= new List<DifficultySet>(_difficultyBeatmapSets);//if difficultySets == null, make new list and add array

            difficultySets.Remove(set);
        }
        if (difficultySets != null)
        {
            _difficultyBeatmapSets = difficultySets.ToArray();
        }*/

        for (var i = 0; i < _difficultyBeatmapSets.Length; i++)
        {
            var mapName = _difficultyBeatmapSets[i].BeatMapName;
            if (!string.IsNullOrWhiteSpace(mapName))
            {
                if (_difficultyBeatmapSets[i].MapGameMode == GameMode.Unset)
                {
                    _difficultyBeatmapSets[i].SetMapGameMode(mapName.GetGameMode());
                    madeChange = true;
                }

                if (_difficultyBeatmapSets[i].MapGameMode == GameMode.Normal)
                {
                    _difficultyBeatmapSets[i].TryCreateMissingDifficulties();
                    difficultySet = _difficultyBeatmapSets[i];
                }

                if (_difficultyBeatmapSets[i].MapGameMode is GameMode.Degrees90 or GameMode.Degrees360)
                {
                    if (_difficultyBeatmapSets[i].MapGameMode is GameMode.Degrees90)
                    {
                        has90RotationSet = true;
                        rotation90Set = _difficultyBeatmapSets[i];
                    }

                    if (_difficultyBeatmapSets[i].MapGameMode is GameMode.Degrees360)
                    {
                        has360RotationSet = true;
                        rotation360Set = _difficultyBeatmapSets[i];
                    }
                }
            }
        }

        var length = (int) GameMode.NoObstacles;
        if (_difficultyBeatmapSets.Length < length)
        {
            var maps = new DifficultySet[length];
            for (int i = 0; i < _difficultyBeatmapSets.Length; i++)
            {
                maps[i] = _difficultyBeatmapSets[i];
            }

            _difficultyBeatmapSets = maps;
        }

        for (var index = 1; index <= length; index++)
        {
            var gameMode = (GameMode) index;

            var hasSet = false;
            for (var i = 0; i < _difficultyBeatmapSets.Length; i++)
            {
                if (_difficultyBeatmapSets[i].MapGameMode == gameMode)
                {
                    hasSet = true;
                    break;
                }
            }

            if (hasSet)
            {
                continue;
            }

            madeChange = true;
            for (var i = 0; i < _difficultyBeatmapSets.Length; i++)
            {
                if (_difficultyBeatmapSets[i].MapGameMode == GameMode.Unset)
                {
                    _difficultyBeatmapSets[i] = difficultySet;
                    string newFileName = null;
                    switch (gameMode)
                    {
                        case GameMode.Unset:
                            break;
                        case GameMode.Normal:
                            break;
                        case GameMode.JabsOnly:
                            break;
                        case GameMode.OneHanded:
                            break;
                        case GameMode.Degrees90:
                            if (has90RotationSet)
                            {
                                _difficultyBeatmapSets[i] = rotation90Set;
                            }
                            else if (createdRotationSet)
                            {
                                _difficultyBeatmapSets[i] = rotation360Set;
                            }
                            else
                            {
                                newFileName =
                                    await AsyncCreateNewDifficultyFile(difficultySet.DifficultyInfos[^1], gameMode,
                                        token);

                                _difficultyBeatmapSets[i].SetFileName(newFileName);
                                rotation90Set = _difficultyBeatmapSets[i];
                                has90RotationSet = true;
                                createdRotationSet = true;
                            }

                            break;
                        case GameMode.Degrees360:
                            if (has360RotationSet)
                            {
                                _difficultyBeatmapSets[i] = rotation360Set;
                            }
                            else if (createdRotationSet)
                            {
                                _difficultyBeatmapSets[i] = rotation90Set;
                            }
                            else
                            {
                                newFileName =
                                    await AsyncCreateNewDifficultyFile(difficultySet.DifficultyInfos[^1], gameMode,
                                        token);

                                _difficultyBeatmapSets[i].SetFileName(newFileName);
                                rotation360Set = _difficultyBeatmapSets[i];
                                has360RotationSet = true;
                                createdRotationSet = true;
                            }

                            break;
                        case GameMode.LightShow:
                            break;
                        case GameMode.LegDay:
                            newFileName =
                                await AsyncCreateNewDifficultyFile(difficultySet.DifficultyInfos[^1], gameMode,
                                    token);
                            _difficultyBeatmapSets[i].SetFileName(newFileName);
                            break;
                        case GameMode.NoObstacles:
                            break;
                        case GameMode.Lawless:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    _difficultyBeatmapSets[i].SetMapGameMode(gameMode);
                    break;
                }
            }
        }

        for (var i = 0; i < _difficultyBeatmapSets.Length; i++)
        {
            var removed = _difficultyBeatmapSets[i].TryRemoveExpertPlus();
            if (removed)
            {
                madeChange = true;
            }
        }

        return madeChange;
    }

    public void SetDifficultySets(DifficultySet[] sets)
    {
        _difficultyBeatmapSets = sets;
    }

    private async UniTask<string> AsyncCreateNewDifficultyFile(DifficultyInfo info, GameMode mode,
        CancellationToken token)
    {
        var choreography = await Choreography.AsyncLoadFromSongInfo(this, info, token);
        var newFileName = info.FileName.Substring(0, info.FileName.LastIndexOf('.'));
        switch (mode)
        {
            case GameMode.Unset:
                break;
            case GameMode.Normal:
                break;
            case GameMode.JabsOnly:
                break;
            case GameMode.NoObstacles:
                break;
            case GameMode.OneHanded:
                break;
            case GameMode.Degrees90:
            case GameMode.Degrees360:
                newFileName = $"AutoRotation-{newFileName}.dat";
                choreography = await choreography.AddRotationEventsAsync();
                await Choreography.AsyncSave(choreography, fileLocation, newFileName, _songName, token);
                break;
            case GameMode.LightShow:
                break;
            case GameMode.LegDay:
                newFileName = $"LegDay-{newFileName}.dat";
                choreography = await choreography.AddObstaclesAsync(this);
                await Choreography.AsyncSave(choreography, fileLocation, newFileName, _songName, token);
                break;
            case GameMode.Lawless:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }


        return newFileName;
    }

    public async UniTask<Sprite> LoadImage(CancellationToken token)
    {
        if (_songArt != null)
        {
            return _songArt;
        }

        Texture2D image;
        if (isCustomSong)
        {
            image = await AssetManager.LoadCustomSongImage(fileLocation, this, token);
        }
        else
        {
            //await UniTask.SwitchToMainThread(token);
            image = await AssetManager.LoadBuiltInSongImage(this, token);
        }

        if (image == null)
        {
            return null;
        }
        
        _songArt = Sprite.Create(image, new Rect(0,0, image.width, image.height),
            Vector2.one *.5f, 100f);
        return _songArt;
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

        [SerializeField]
        private string _beatmapCharacteristicName;

        [SerializeField]
        private GameMode _mapGameMode;

        public GameMode MapGameMode =>
            _mapGameMode == GameMode.Unset ? _mapGameMode = BeatMapName.GetGameMode() : _mapGameMode;

        public DifficultyInfo[] DifficultyInfos => _difficultyBeatmaps;

        public string BeatMapName
        {
            get { return _beatmapCharacteristicName ?? GameMode.Unset.GetDifficultySetName(); }
        }

        public void SetMapGameMode(GameMode gameMode)
        {
            _mapGameMode = gameMode;
            _beatmapCharacteristicName = gameMode.GetDifficultySetName();
        }

        public void TryCreateMissingDifficulties()
        {
            var hardestSet = new DifficultyInfo();
            var easyInfo = new DifficultyInfo();
            var hasEasy = false;
            var normalInfo = new DifficultyInfo();
            var hasNormal = false;
            var hardInfo = new DifficultyInfo();
            var hasHard = false;
            var expertInfo = new DifficultyInfo();
            var hasExpert = false;

            foreach (var difficulty in _difficultyBeatmaps)
            {
                switch (true) // determine which difficulties this set has
                {
                    case var b when difficulty.DifficultyRank <= DifficultyInfo.EASY:
                        hasEasy = true;
                        easyInfo = difficulty;
                        break;
                    case var b when difficulty.DifficultyRank <= DifficultyInfo.NORMAL:
                        hasNormal = true;
                        normalInfo = difficulty;
                        break;
                    case var b when difficulty.DifficultyRank <= DifficultyInfo.HARD:
                        hasHard = true;
                        hardInfo = difficulty;
                        break;
                    case var b when difficulty.DifficultyRank <= DifficultyInfo.EXPERT:
                        hasExpert = true;
                        expertInfo = difficulty;
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

            _difficultyBeatmaps = new DifficultyInfo[4];
            if (!hasEasy)
            {
                easyInfo = SetDifficultyName(hardestSet, DifficultyInfo.EASY);
            }

            if (!hasNormal)
            {
                normalInfo = SetDifficultyName(hardestSet, DifficultyInfo.NORMAL);
            }

            if (!hasHard)
            {
                hardInfo = SetDifficultyName(hardestSet, DifficultyInfo.HARD);
            }

            if (!hasExpert)
            {
                expertInfo = SetDifficultyName(hardestSet, DifficultyInfo.EXPERT);
            }

            _difficultyBeatmaps[0] = easyInfo;
            _difficultyBeatmaps[1] = normalInfo;
            _difficultyBeatmaps[2] = hardInfo;
            _difficultyBeatmaps[3] = expertInfo;
        }

        private DifficultyInfo SetDifficultyName(DifficultyInfo info, int difficulty)
        {
            var newDifficulty = info;
            switch (difficulty)
            {
                case DifficultyInfo.EASY:
                    newDifficulty = newDifficulty.SetDifficulty($"{AUTONAME}{EASY}", DifficultyInfo.EASY,
                        info.DifficultyRank > DifficultyInfo.EASY);
                    break;
                case DifficultyInfo.NORMAL:
                    newDifficulty = newDifficulty.SetDifficulty($"{AUTONAME}{NORMAL}", DifficultyInfo.NORMAL,
                        info.DifficultyRank > DifficultyInfo.NORMAL);
                    break;
                case DifficultyInfo.HARD:
                    newDifficulty = newDifficulty.SetDifficulty($"{AUTONAME}{HARD}", DifficultyInfo.HARD,
                        info.DifficultyRank > DifficultyInfo.HARD);
                    break;
                case DifficultyInfo.EXPERT:
                    newDifficulty = newDifficulty.SetDifficulty($"{AUTONAME}{EXPERT}", DifficultyInfo.EXPERT,
                        info.DifficultyRank > DifficultyInfo.EXPERT);
                    break;
            }

            return newDifficulty;
        }

        public bool TryRemoveExpertPlus()
        {
            var removed = false;

            if (_difficultyBeatmaps.Length - 1 > -1 &&
                _difficultyBeatmaps[^1].DifficultyRank >= DifficultyInfo.EXPERTPLUS) // ^1 is the last in array
            {
                var newArray = new DifficultyInfo[_difficultyBeatmaps.Length - 1];
                for (var i = 0; i < newArray.Length; i++)
                {
                    newArray[i] = _difficultyBeatmaps[i];
                }

                _difficultyBeatmaps = newArray;
                removed = true;
            }

            return removed;
        }

        public void SetFileName(string fileName)
        {
            var beatmaps = new DifficultyInfo[_difficultyBeatmaps.Length];
            for (var i = 0; i < _difficultyBeatmaps.Length; i++)
            {
                beatmaps[i] = _difficultyBeatmaps[i];
                beatmaps[i] = beatmaps[i].SetFileName(fileName);
            }

            _difficultyBeatmaps = beatmaps;
        }
    }

    public struct UnManagedDifficultySet
    {
        [SerializeField]
        private GameMode _mapGameMode;

        public GameMode MapGameMode => _mapGameMode;
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