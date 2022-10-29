using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[Serializable]
[BurstCompile]
public struct ChoreographyObstacle// : ISequenceable
{
    public float Time => _time;
    public float Duration => _duration;
    public ObstacleType Type => _type;
    public int LineIndex => _lineIndex;
    public int Width => _width;

    [SerializeField]
    private float _time;

    [SerializeField]
    private float _duration;

    [SerializeField]
    private ObstacleType _type;

    [SerializeField]
    private int _lineIndex;

    [SerializeField]
    private int _width; //This isn't actually used.

    public HitSideType HitSideType => (_type == ObstacleType.Dodge
        ? _lineIndex <= 1 ? HitSideType.Left : HitSideType.Right
        : HitSideType.Block);


    public enum ObstacleType
    {
        Dodge = 0,
        Crouch = 1
    }

    public ChoreographyObstacle(float time, float duration, ObstacleType type, int lineIndex, int width)
    {
        _time = time;
        _duration = duration;
        _type = type;
        _lineIndex = lineIndex;
        _width = width;
    }
}