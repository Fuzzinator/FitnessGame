using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DifficultyInfo
{
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

    [SerializeField]
    private float _minTargetSpace;

    public float MinTargetSpace => _minTargetSpace;

    #region Consts

    private const float EASYDISTANCE = .75f;
    private const float NORMALDISTANCE = .5f;
    private const float HARDDISTANCE = .25f;
    private const float EXPERTDISTANCE = 15f;

    private const float MINEASYSPEED = 9;
    private const float MAXEASYSPEED = 10;
    private const float MINNORMALSPEED = 11;
    private const float MAXNORMALSPEED = 12;
    private const float MINHARDSPEED = 13;
    private const float MAXHARDSPEED = 14;
    private const float MINEXPERTSPEED = 15;
    private const float MAXEXPERTSPEED = 16;

    public const int EASY = 1;
    public const int NORMAL = 3;
    public const int HARD = 5;
    public const int EXPERT = 7;

    #endregion

    public void SetMinDistance()
    {
        _minTargetSpace = true switch
        {
            true when _difficultyRank <= EASY => EASYDISTANCE,
            true when _difficultyRank <= NORMAL => NORMALDISTANCE,
            true when _difficultyRank <= HARD => HARDDISTANCE,
            true when _difficultyRank <= EXPERT => EXPERTDISTANCE,
            _ => _minTargetSpace
        };
    }

    public void SetDifficulty(string difficultyName, int difficultyRank)
    {
        _difficulty = difficultyName;
        _difficultyRank = difficultyRank;
    }
}