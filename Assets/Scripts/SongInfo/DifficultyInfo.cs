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

    public float MovementSpeed => _noteJumpMovementSpeed;

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

    private const string EASY = "Easy";
    private const string NORMAL = "Normal";
    private const string HARD = "Hard";
    private const string EXPERT = "Expert";

    #endregion

    public void SetMinDistance()
    {
        _minTargetSpace = _difficulty switch
        {
            EASY => EASYDISTANCE,
            NORMAL => NORMALDISTANCE,
            HARD => HARDDISTANCE,
            EXPERT => EXPERTDISTANCE,
            _ => _minTargetSpace
        };
    }
}