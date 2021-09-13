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
    private string _beatmapFileName;

    public string FileName => _beatmapFileName;

    [SerializeField]
    private float _noteJumpMovementSpeed;

    public float MovementSpeed => _noteJumpMovementSpeed;

    [SerializeField]
    private float _noteJumpStartBeatOffset;

    public float BeatOffset => _noteJumpStartBeatOffset;
}