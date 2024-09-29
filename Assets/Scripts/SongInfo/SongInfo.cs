using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using BeatSaverSharp.Models;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using GameModeManagement;
using Unity.Burst;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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

    public float LengthInMinutes => SongLength / MINUTE;

    public string GetReadableLength(float speedMod = 1f)
    {
        var length = SongLength / speedMod;
        var minutes = Mathf.Floor(length / MINUTE);
        var seconds = Mathf.Floor(length % MINUTE);
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

    public string Genre => _genre;

    public string Attribution => _attribution;

    public string ImageFilename => _coverImageFilename;

    public string SongID => _songID;

    public float SongScore => _songScore;

    public bool IsHidden
    {
        get
        {
            return HiddenAssetManager.IsHiddenSong(_songID);
        }
    }

    public DateTime DownloadedDate
    {
        get
        {
            if (_downloadedDate == null)
            {
                _downloadedDate = DateTime.MinValue;
            }
            return _downloadedDate;
        }
        set
        {
            _downloadedDate = value;
        }
    }

    //public float Score;

    public DifficultySet[] DifficultySets => _difficultyBeatmapSets;

    public string RecordableName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_songID))
            {
                return _songID.RemoveSpecialCharacters();
            }
            return ($"{_songName}{_songLength}").RemoveSpecialCharacters();
        }
    }

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

    public bool isCustomSong;

    [SerializeField]
    private float _songLength;

    [SerializeField]
    private string _genre;

    [SerializeField]
    private string _attribution;

    [SerializeField]
    private string _songID;

    [SerializeField]
    private float _songScore = -1;

    [SerializeField]
    private DifficultySet[] _difficultyBeatmapSets;

    private Sprite _songArt;

    private DateTime _downloadedDate;

    private AsyncOperationHandle _textureLoadHandle;

    private const string FMT = "O";

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

    public struct UpdatedMaps
    {
        public bool MadeChange { get; private set; }
        public bool Success { get; private set; }

        public UpdatedMaps(bool madeChange, bool success)
        {
            MadeChange = madeChange;
            Success = success;
        }
    }

    public async UniTask<UpdatedMaps> UpdateDifficultySets(CancellationToken token)
    {
        var changeMade = false;
        var difficultySet = new DifficultySet();
        var has90RotationSet = false;
        var has360RotationSet = false;
        var createdRotationSet = false;
        var rotation90Set = new DifficultySet();
        var rotation360Set = new DifficultySet();
        var hasNormal = false;

        foreach (var set in _difficultyBeatmapSets)
        {
            if (set.MapGameMode == GameMode.Normal)
            {
                hasNormal = true;
                break;
            }
        }

        for (var i = 0; i < _difficultyBeatmapSets.Length; i++)
        {
            var mapName = _difficultyBeatmapSets[i].BeatMapName;
            if (!string.IsNullOrWhiteSpace(mapName))
            {
                switch (_difficultyBeatmapSets[i].MapGameMode)
                {
                    case GameMode.Unset:
                        _difficultyBeatmapSets[i].SetMapGameMode(mapName.GetGameMode());
                        changeMade = true;
                        break;
                    case GameMode.Normal:
                        _difficultyBeatmapSets[i].TryCreateMissingDifficulties();
                        difficultySet = _difficultyBeatmapSets[i];
                        break;
                    case GameMode.JabsOnly:
                        if (hasNormal)
                        {
                            break;
                        }
                        _difficultyBeatmapSets[i].TryCreateMissingDifficulties();
                        difficultySet = _difficultyBeatmapSets[i];
                        break;
                    case GameMode.OneHanded:
                        if (hasNormal)
                        {
                            break;
                        }
                        var result = await TryCreateNormalFrom1Handed(this, _difficultyBeatmapSets[i].DifficultyInfos, token);
                        if (!string.IsNullOrWhiteSpace(result.BeatMapName))
                        {
                            difficultySet = result;
                            hasNormal = true;
                        }
                        break;
                    case GameMode.Degrees90:
                        has90RotationSet = true;
                        rotation90Set = _difficultyBeatmapSets[i];
                        break;
                    case GameMode.Degrees360:
                        has360RotationSet = true;
                        rotation360Set = _difficultyBeatmapSets[i];
                        break;
                    case GameMode.LegDay:
                        break;
                    case GameMode.NoObstacles:
                        break;
                    case GameMode.Lawless:
                        if (hasNormal || difficultySet.DifficultyInfos != null)
                        {
                            break;
                        }

                        difficultySet = _difficultyBeatmapSets[i];

                        break;
                    default: break;
                }
            }
        }

        var length = (int)GameMode.NoObstacles;
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
            var gameMode = (GameMode)index;

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

            for (var i = 0; i < _difficultyBeatmapSets.Length; i++)
            {
                if (_difficultyBeatmapSets[i].MapGameMode is GameMode.Unset or GameMode.LightShow or GameMode.Lawless)
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

                                if (string.IsNullOrWhiteSpace(newFileName))
                                {
                                    return new UpdatedMaps(false, false);
                                }
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

                                if (string.IsNullOrWhiteSpace(newFileName))
                                {
                                    return new UpdatedMaps(false, false);
                                }
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

                            if (string.IsNullOrWhiteSpace(newFileName))
                            {
                                return new UpdatedMaps(false, false);
                            }
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

                    changeMade = true;
                    break;
                }
            }
        }

        /*for (var i = 0; i < _difficultyBeatmapSets.Length; i++)
        {
            var removed = _difficultyBeatmapSets[i].TryRemoveExpertPlus();
            if (removed)
            {
                changeMade = true;
            }
        }*/

        return new UpdatedMaps(changeMade, true);
    }

    public void SetDifficultySets(DifficultySet[] sets)
    {
        _difficultyBeatmapSets = sets;
    }

    public void SetBPS(int newBPS)
    {
        _beatsPerMinute = newBPS;
    }

    private async UniTask<string> AsyncCreateNewDifficultyFile(DifficultyInfo info, GameMode mode,
        CancellationToken token)
    {
        var choreography = await Choreography.AsyncLoadFromSongInfo(this, info, token);
        if (choreography == null)
        {
            return null;
        }
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

    public async UniTask AsyncAddObstacles(DifficultyInfo info, GameMode mode,
        CancellationToken token)
    {
        var choreography = await Choreography.AsyncLoadFromSongInfo(this, info, token);
        if (choreography == null)
        {
            return;
        }
        switch (mode)
        {
            case GameMode.Unset:
                break;
            case GameMode.Normal:
            case GameMode.JabsOnly:
            case GameMode.NoObstacles:
            case GameMode.OneHanded:
            case GameMode.Degrees90:
            case GameMode.Degrees360:
                choreography = await choreography.AddObstaclesAsync(this, 30);
                await Choreography.AsyncSave(choreography, fileLocation, info.FileName, _songName, token);
                break;
            case GameMode.LightShow:
                break;
            case GameMode.LegDay:
                choreography = await choreography.AddObstaclesAsync(this);
                await Choreography.AsyncSave(choreography, fileLocation, info.FileName, _songName, token);
                break;
            case GameMode.Lawless:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    public async UniTask AsyncAddRotations(DifficultyInfo info, GameMode mode,
        CancellationToken token)
    {
        var choreography = await Choreography.AsyncLoadFromSongInfo(this, info, token);
        if (choreography == null)
        {
            return;
        }
        switch (mode)
        {
            case GameMode.Unset:
            case GameMode.Normal:
            case GameMode.JabsOnly:
            case GameMode.NoObstacles:
            case GameMode.OneHanded:
                break;
            case GameMode.Degrees90:
            case GameMode.Degrees360:
                choreography = await choreography.AddRotationEventsAsync();
                await Choreography.AsyncSave(choreography, fileLocation, info.FileName, _songName, token);
                break;
            case GameMode.LightShow:
            case GameMode.LegDay:
            case GameMode.Lawless:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }


        return;
    }

    public async UniTask<Sprite> LoadImage(CancellationToken token)
    {
        if (_songArt != null)
        {
            return _songArt;
        }

        var image = await LoadTexture(token);

        if (image == null)
        {
            return null;
        }

        _songArt = Sprite.Create(image, new Rect(0, 0, image.width, image.height),
            Vector2.one * .5f, 100f, 0, SpriteMeshType.FullRect);
        return _songArt;
    }

    public async UniTask<Texture2D> LoadTexture(CancellationToken token)
    {
        if (_songArt != null)
        {
            return _songArt.texture;
        }

        Texture2D image;
        if (isCustomSong)
        {
            image = await AssetManager.LoadCustomSongImage(fileLocation, this, token);
        }
        else
        {
            //await UniTask.SwitchToMainThread(token);
            var imageRequest = await AssetManager.LoadBuiltInSongImage(this, token);
            _textureLoadHandle = imageRequest.OperationHandle;
            image = imageRequest.Texture;
        }

        return image;
    }

    public void SetImage(Texture2D texture)
    {
        _songArt = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
            Vector2.one * .5f, 100f, 0, SpriteMeshType.FullRect);
    }

    public void SetSongID(string id)
    {
        _songID = id;
    }

    public void SetSongScore(float songScore)
    {
        _songScore = songScore;
    }

    public void UnloadImage()
    {
        if (!_textureLoadHandle.IsValid())
        {
            return;
        }
        Addressables.Release(_textureLoadHandle);
        if (_songArt != null)
        {
            _songArt = null;
        }
    }

    public void ConvertFromBeatSage()
    {
        _levelAuthorName = "3Pupper Studios";
        _songID = _songName;
        foreach (var set in _difficultyBeatmapSets)
        {
            for (var i = set.DifficultyInfos.Length - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                    set.DifficultyInfos[i] = new DifficultyInfo("Easy", 1, set.DifficultyInfos[i].FileName.Replace("Normal", "Easy"), 7f);
                }
                else
                {
                    set.DifficultyInfos[i] = set.DifficultyInfos[i - 1];
                }
            }
        }
    }
    public void ConvertToLocal()
    {
        foreach (var set in _difficultyBeatmapSets)
        {
            for (var i = set.DifficultyInfos.Length - 1; i >= 0; i--)
            {
                var info = set.DifficultyInfos[i];
                set.DifficultyInfos[i] = new DifficultyInfo(info.Difficulty, info.DifficultyRank, info.FileName.Replace(".dat", ".txt"), info.MovementSpeed);
            }
        }
    }


    public async static UniTask<DifficultySet> TryCreateNormalFrom1Handed(SongInfo songInfo, DifficultyInfo[] difficultyBeatmaps, CancellationToken token)
    {
        if (difficultyBeatmaps == null)
        {
            return new DifficultySet();
        }
        var hardestSet = new DifficultyInfo();

        foreach (var difficultyMap in difficultyBeatmaps)
        {
            if (difficultyMap.DifficultyRank > hardestSet.DifficultyRank)
            {
                hardestSet = difficultyMap;
            }
        }

        var newFileName = $"{songInfo.SongName}-AutoNormal.dat";
        await RandomizeAndSaveChoreography(songInfo, hardestSet, newFileName, token);
        hardestSet = DifficultyInfo.SetFileName(hardestSet, newFileName);

        var infos = new DifficultyInfo[4];
        var difficulty = 1;
        for (var i = 0; i < infos.Length; i++)
        {
            infos[i] = DifficultySet.SetDifficultyName(hardestSet, difficulty);
            difficulty += 2;
        }
        return new DifficultySet(infos, "Normal", GameMode.Normal);
    }

    private static async UniTask RandomizeAndSaveChoreography(SongInfo songInfo, DifficultyInfo difficultyInfo, string newFileName, CancellationToken token)
    {
        var choreography = await Choreography.AsyncLoadFromSongInfo(songInfo, difficultyInfo, token);

        var seed = UnityEngine.Random.state;
        UnityEngine.Random.InitState(choreography.Notes.Length + choreography.Obstacles.Length + (int)(songInfo.SongLength * 100));
        for (var i = 0; i < choreography.Notes.Length; i++)
        {
            var note = choreography.Notes[i];
            var random = UnityEngine.Random.Range(0, 4);
            if (random == 2)
            {
                random = UnityEngine.Random.Range(0, 2);
            }
            choreography.Notes[i] = note.SetType((HitSideType)random);
        }
        UnityEngine.Random.state = seed;

        await Choreography.AsyncSave(choreography, songInfo.fileLocation, newFileName, songInfo.SongName, token);
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

        public DifficultySet(DifficultyInfo[] difficultyBeatmaps, string mapName, GameMode gameMode)
        {
            _difficultyBeatmaps = difficultyBeatmaps;
            _beatmapCharacteristicName = mapName;
            _mapGameMode = gameMode;
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

        public static DifficultyInfo SetDifficultyName(DifficultyInfo info, int difficulty)
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
                beatmaps[i] = DifficultyInfo.SetFileName(_difficultyBeatmaps[i], fileName);
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
        InverseSongLength = 6,
        LevelAuthorName = 7,
        InverseLevelAuthorName = 8,
        BPM = 9,
        InverseBPM = 10,
        SongScore = 11,
        InverseSongScore = 12,
        RecentlyDownloaded = 13,
        InverseRecentlyDownloaded = 14,
    }

    public static bool operator ==(SongInfo info, PlaylistItem item)
    {
        if (!string.IsNullOrWhiteSpace(info.SongID) && !string.IsNullOrWhiteSpace(item.SongID))
        {
            return string.Equals(info.SongID, item.SongID);
        }
        return info.SongName == item.SongName && info.fileLocation == item.FileLocation &&
               info.isCustomSong == item.IsCustomSong;
    }

    public static bool operator !=(SongInfo info, PlaylistItem item)
    {
        return !(info == item);
    }

    public static bool operator ==(SongInfo info, Beatmap beatmap)
    {
        return beatmap == info;
    }

    public static bool operator !=(SongInfo info, Beatmap beatmap)
    {
        return beatmap != info;
    }

    public static bool operator ==(Beatmap beatmap, SongInfo songInfo)
    {
        if (beatmap is null && songInfo is null)
        {
            return true;
        }
        if (beatmap is null || songInfo is null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(beatmap.ID) && !string.IsNullOrWhiteSpace(songInfo.SongID))
        {
            return string.Equals(beatmap.ID, songInfo.SongID);
        }
        var metaData = beatmap.Metadata;
        var songNameMatches = string.Equals(metaData.SongName, songInfo.SongName);
        var songAuthorMatches = SongAuthorMatches(metaData, songInfo, out var badSongAuthorData);
        var levelAuthorMatches = LevelAuthorMatches(metaData, songInfo, out var badLevelAuthorData);
        return songNameMatches &&
               ((songAuthorMatches && levelAuthorMatches) ||
                (songAuthorMatches && badSongAuthorData) ||
                (levelAuthorMatches && badLevelAuthorData));
    }

    public static bool operator !=(Beatmap beatmap, SongInfo songInfo)
    {
        return !(beatmap == songInfo);
    }

    /// <summary>
    /// I dont reccomend using this if comparing against a <see cref="PlaylistItem"/> because
    /// that will result in boxing and creating needless garbage. Instead use ==.
    /// </summary>
    /// <param name="obj">Object to compare.</param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        if (obj is PlaylistItem item)
        {
            return this == item;
        }
        else if (obj is Beatmap beatmap)
        {
            return this == beatmap;
        }

        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    private static bool SongAuthorMatches(BeatmapMetadata metaData, SongInfo songInfo, out bool badMetaData)
    {
        badMetaData = false;

        var songAuthorMatches = string.Equals(metaData.SongAuthorName, songInfo.SongAuthorName);
        if (!songAuthorMatches)// This exists to prevent false negatives when beatmap meta data is wrong
        {
            songAuthorMatches = string.Equals(metaData.SongAuthorName, songInfo.LevelAuthorName);
            if (songAuthorMatches)
            {
                badMetaData = true;
            }
        }

        return songAuthorMatches;
    }

    private static bool LevelAuthorMatches(BeatmapMetadata metaData, SongInfo songInfo, out bool badMetaData)
    {
        badMetaData = false;

        var levelMatches = string.Equals(metaData.LevelAuthorName, songInfo.LevelAuthorName);
        if (!levelMatches)// This exists to prevent false negatives when beatmap meta data is wrong
        {
            levelMatches = string.Equals(metaData.LevelAuthorName, songInfo.SongAuthorName);
            if (levelMatches)
            {
                badMetaData = true;
            }
        }

        return levelMatches;
    }

}