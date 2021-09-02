using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ChoreographyNote : ISequenceable
{
    public float Time => _time;
    public int LineIndex => _lineIndex;
    public LineLayerType LineLayer => _lineLayer;
    public HitSideType Type => _type;
    public CutDirection CutDir => _cutDirection;

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

    public HitSideType HitSideType => _type;

    public enum LineLayerType
    {
        Low = 0,
        Middle = 1,
        High = 2
    }


    public enum CutDirection
    {
        Uppercut = 0,
        JabDown = 1,//Will be treated as Jab(8) {Only exists for BeatSaber map support}
        HookLeft = 2,//Only allowed on NoteType.Right
        HookRight = 3,//Only allowed on NoteType.Left
        UppercutLeft = 4,//Only allowed on NoteType.Right {Experimental}
        UppercutRight = 5,//Only allowed on NoteType.Left {Experimental}
        HookLeftDown = 6,//Will be treated as HookLeft(2) {Only exists for BeatSaber map support}
        HookRightDown = 7,//Will be treated as HookRight(3) {Only exists for BeatSaber map support}
        Jab = 8
    }
}


public enum HitSideType
{
    Left = 0,
    Right = 1,
    Unused = 2,
    Block = 3
}