using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[Serializable]
[BurstCompile]
public struct ChoreographyNote// : ISequenceable
{
    public float Time => _time;
    public int LineIndex => _lineIndex;
    public LineLayerType LineLayer => _lineLayer;
    public HitSideType Type => _type;
    public CutDirection CutDir => _cutDirection;
    public HitSideType HitSideType => _type;

    [SerializeField]
    private float _time;
    [SerializeField]
    private int _lineIndex;
    [SerializeField]
    private LineLayerType _lineLayer;
    [SerializeField]
    private HitSideType _type;
    [SerializeField]
    private CutDirection _cutDirection;
    
    public ChoreographyNote(float time, int lineIndex, LineLayerType lineLayer, HitSideType hitSide,
        CutDirection cutDirection)
    {
        _time = time;
        _lineIndex = lineIndex;
        _lineLayer = lineLayer;
        _type = hitSide;
        _cutDirection = cutDirection;
    }

    public enum LineLayerType
    {
        Low = 0,
        Middle = 1,
        High = 2
    }

    public ChoreographyNote SetToBasicJab()
    {
        SetCutDirection(CutDirection.Jab);
        SetLineLayer(LineLayerType.Middle);
        SetLineIndex(_type == HitSideType.Left || _type == HitSideType.Block? 1:2);
        return this;
    }

    public ChoreographyNote SetToBlock()
    {
        SetCutDirection(CutDirection.Jab);
        //SetLineLayer(LineLayerType.Middle);
        SetLineIndex(1);
        SetType(HitSideType.Block);
        return this;
    }

    public ChoreographyNote SetCutDirection(CutDirection direction)
    {
        _cutDirection = direction;
        return this;
    }

    public ChoreographyNote SetLineLayer(LineLayerType layerType)
    {
        _lineLayer = layerType;
        return this;
    }

    public ChoreographyNote SetLineIndex(int index)
    {
        _lineIndex = index;
        return this;
    }

    public ChoreographyNote SetType(HitSideType type)
    {
        _type = type;
        return this;
    }
    
    public ChoreographyNote SwapSides()
    {
        _type = _type switch
        {
            HitSideType.Left => HitSideType.Right,
            HitSideType.Right => HitSideType.Left,
            HitSideType.Unused => HitSideType.Unused,
            HitSideType.Block => HitSideType.Block,
            _ => _type
        };
        return this;
    }

    public enum CutDirection
    {
        Uppercut = 0,
        JabDown = 1,//Will be treated as Jab(8) {Only exists for BeatSaber map support}
        HookLeft = 2,//Only allowed on NoteType.Right
        HookRight = 3,//Only allowed on NoteType.Left
        UppercutLeft = 4,//Treated as Uppercut(0) {Only exists for BeatSaber map support}
        UppercutRight = 5,//Treated as Uppercut(0) {Only exists for BeatSaber map support}
        HookLeftDown = 6,//Will be treated as Jab(8) {Only exists for BeatSaber map support}
        HookRightDown = 7,//Will be treated as Jab(8) {Only exists for BeatSaber map support}
        Jab = 8
    }
}

[Serializable]
public enum HitSideType
{
    Left = 0,
    Right = 1,
    Unused = 2,
    Block = 3
}