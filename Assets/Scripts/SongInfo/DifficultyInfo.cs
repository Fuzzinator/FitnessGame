using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DifficultyInfo
{
    public DifficultyInfo(string difficulty, int rank, string fileName, float noteSpeed)
    {
        _difficulty = difficulty;
        _difficultyRank = rank;
        _beatmapFilename = fileName;
        _noteJumpMovementSpeed = noteSpeed;
        _noteJumpStartBeatOffset = 0f;
    }

    [SerializeField]
    private string _difficulty;

    public string Difficulty => _difficulty;

    [SerializeField]
    private int _difficultyRank;

    public int DifficultyRank => _difficultyRank;

    [SerializeField]
    private string _beatmapFilename;

    public string FileName => _beatmapFilename;

    [SerializeField]
    private float _noteJumpMovementSpeed;

    public float MovementSpeed
    {
        get
        {
            return true switch
            {
                true when _difficultyRank <= EASY => Mathf.Clamp(_noteJumpMovementSpeed, MINEASYSPEED, MAXEASYSPEED),
                true when _difficultyRank > EASY && _difficultyRank <= NORMAL => Mathf.Clamp(_noteJumpMovementSpeed,
                    MINNORMALSPEED, MAXNORMALSPEED),
                true when _difficultyRank > NORMAL && _difficultyRank <= HARD => Mathf.Clamp(_noteJumpMovementSpeed,
                    MINHARDSPEED, MAXHARDSPEED),
                true when _difficultyRank > HARD && _difficultyRank <= EXPERT => Mathf.Clamp(_noteJumpMovementSpeed,
                    MINEXPERTSPEED, MAXEXPERTSPEED),
                _ => _noteJumpMovementSpeed
            };
        }
    }

    [SerializeField]
    private float _noteJumpStartBeatOffset;

    public float BeatOffset => _noteJumpStartBeatOffset;

    public DifficultyEnum DifficultyAsEnum => GetDifficultyAsEnum(_difficultyRank);

    public static DifficultyEnum GetDifficultyAsEnum(int difficulty)
    {
        return true switch
        {
            var b when difficulty <= 0 => DifficultyEnum.Unset,
            var b when difficulty <= EASY => DifficultyEnum.Easy,
            var b when difficulty <= NORMAL => DifficultyEnum.Normal,
            var b when difficulty <= HARD => DifficultyEnum.Hard,
            var b when difficulty <= EXPERT => DifficultyEnum.Expert,
            _ => DifficultyEnum.Unset
        };
    }

    public float MinTargetSpace
    {
        get
        { 
            return true switch
            {
                true when _difficultyRank <= EASY => EASYDISTANCE,
                true when _difficultyRank <= NORMAL => NORMALDISTANCE,
                true when _difficultyRank <= HARD => HARDDISTANCE,
                true when _difficultyRank <= EXPERT => EXPERTDISTANCE,
                _ => NORMALDISTANCE
            };
        }
    }
    //_minTargetSpace;

    #region Consts

    private const float EASYDISTANCE = 1f;
    private const float NORMALDISTANCE = .75f;
    private const float HARDDISTANCE = .5f;
    private const float EXPERTDISTANCE = .25f;

    private const float MINEASYSPEED = 7;
    private const float MAXEASYSPEED = 9;
    private const float MINNORMALSPEED = 10;
    private const float MAXNORMALSPEED = 11;
    private const float MINHARDSPEED = 12;
    private const float MAXHARDSPEED = 13;
    private const float MINEXPERTSPEED = 14;
    private const float MAXEXPERTSPEED = 15;

    public const int EASY = 1;
    public const int NORMAL = 3;
    public const int HARD = 5;
    public const int EXPERT = 7;
    public const int EXPERTPLUS = 9;

    #endregion

    private DifficultyInfo(DifficultyInfo info, string newFileName)
    {
        _difficulty = info._difficulty;
        _difficultyRank = info._difficultyRank;
        _beatmapFilename = newFileName;
        _noteJumpMovementSpeed = info._noteJumpMovementSpeed;
        _noteJumpStartBeatOffset = info._noteJumpStartBeatOffset;
    }
    
    public DifficultyInfo SetDifficulty(string difficultyName, int difficultyRank, bool downScale)
    {
        _difficulty = difficultyName;
        _difficultyRank = difficultyRank;
        _noteJumpMovementSpeed = true switch
        {
            true when _difficultyRank <= EASY => downScale ? MINEASYSPEED : MAXEASYSPEED,
            true when _difficultyRank <= NORMAL => downScale ? MINNORMALSPEED : MAXNORMALSPEED,
            true when _difficultyRank <= HARD => downScale ? MINHARDSPEED : MAXHARDSPEED,
            true when _difficultyRank <= EXPERT => downScale ? MINEXPERTSPEED : MAXEXPERTSPEED,
            _ => MINNORMALSPEED
        };
        return this;
    }

    public DifficultyInfo SetDifficulty(string difficultyName, int difficultyRank, string fileName)
    {
        _difficulty = difficultyName;
        _difficultyRank = difficultyRank;
        _beatmapFilename = fileName;
        _noteJumpMovementSpeed = true switch
        {
            true when _difficultyRank <= EASY => MAXEASYSPEED,
            true when _difficultyRank <= NORMAL =>  MAXNORMALSPEED,
            true when _difficultyRank <= HARD => MAXHARDSPEED,
            true when _difficultyRank <= EXPERT => MAXEXPERTSPEED,
            _ => MINNORMALSPEED
        };
        return this;
    }

    public static DifficultyInfo SetFileName( DifficultyInfo source, string fileName)
    {
        return new DifficultyInfo(source,fileName);
    }
    
    public enum DifficultyEnum
    {
        Unset = 0,
        Easy = 1,
        Normal = 2,
        Hard = 3,
        Expert = 4
    }
}

public static class EnumExtensions
{
    private static string UNSET = "Default";
    private static string EASY = "Easy";
    private static string NORMAL = "Normal";
    private static string HARD = "Hard";
    private static string EXPERT = "Expert";

    public static string Readable(this DifficultyInfo.DifficultyEnum difficultyEnum)
    {
        return difficultyEnum switch
        {
            DifficultyInfo.DifficultyEnum.Unset => UNSET,
            DifficultyInfo.DifficultyEnum.Easy => EASY,
            DifficultyInfo.DifficultyEnum.Normal => NORMAL,
            DifficultyInfo.DifficultyEnum.Hard => HARD,
            DifficultyInfo.DifficultyEnum.Expert => EXPERT,
            _ => ""
        };
    }
}