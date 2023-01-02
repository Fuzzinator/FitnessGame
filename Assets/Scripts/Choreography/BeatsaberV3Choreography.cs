using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[System.Serializable]
public struct BeatsaberV3Choreography
{
    public RotationEvent[] rotationEvents;
    public ChoreographyNote[] colorNotes;
    public BlockNote[] bombNotes;
    public Obstacle[] obstacles;

    public int EventCount => rotationEvents.Length;
    public int NoteCount => colorNotes.Length + bombNotes.Length;
    public int ObstacleCount => obstacles.Length;
    
    /// <summary>
    /// Events that change the current bpm for songs. Currently Unused
    /// </summary>
    [BurstCompile]
    [System.Serializable]
    public struct BpmEvents
    {
        /// <summary>
        /// Time in beats
        /// </summary>
        public float b;

        /// <summary>
        /// Target BPM
        /// </summary>
        public float m;
    }

    /// <summary>
    /// Events that trigger rotations. Currently only events used.
    /// </summary>
    [BurstCompile]
    [System.Serializable]
    public struct RotationEvent
    {
        /// <summary>
        /// Time in beats
        /// </summary>
        public float b;

        /// <summary>
        /// Event Type takes in 0-1 will be used for 14-15
        /// </summary>
        public int e;

        /// <summary>
        /// Clockwise Rotation value
        /// </summary>
        public float r;
    }

    /// <summary>
    /// Now Only used for Left or right side Notes. Blocks are used Separate
    /// </summary>
    [BurstCompile]
    [System.Serializable]
    public struct ChoreographyNote
    {
        /// <summary>
        /// Time in beats
        /// </summary>
        public float b;

        /// <summary>
        /// Unused. In new Beatsaber beatmap its column 0-3 index left to right
        /// </summary>
        public int x;

        /// <summary>
        /// Line Layer Index 0-2 from low to high
        /// </summary>
        public global::ChoreographyNote.LineLayerType y;

        /// <summary>
        /// Left or Right glove. Bombs (Blocks) are moved to new note type
        /// </summary>
        public HitSideType c;

        /// <summary>
        /// Cut Direction
        /// </summary>
        public global::ChoreographyNote.CutDirection d;
    }

    /// <summary>
    /// Choreography Notes that are blocks only
    /// </summary>
    [BurstCompile]
    [System.Serializable]
    public struct BlockNote
    {
        /// <summary>
        /// Time in beats
        /// </summary>
        public float b;

        /// <summary>
        /// Unused. In new Beatsaber beatmap its column 0-3 index left to right
        /// </summary>
        public int x;

        /// <summary>
        /// Line Layer Index 0-2 from low to high
        /// </summary>
        public global::ChoreographyNote.LineLayerType y;

        public HitSideType C => HitSideType.Block;
    }

    /// <summary>
    /// Obstacles
    /// </summary>
    [BurstCompile]
    [System.Serializable]
    public struct Obstacle
    {
        /// <summary>
        /// Time in beats
        /// </summary>
        public float b;
        
        /// <summary>
        /// Unused. In new Beatsaber beatmap its column 0-3 index left to right
        /// </summary>
        public int x;

        /// <summary>
        /// Line Layer Index 0-2 from low to high
        /// </summary>
        public global::ChoreographyNote.LineLayerType y;

        /// <summary>
        /// Duration. Not used
        /// </summary>
        public float d;

        /// <summary>
        /// Width. Not Used
        /// </summary>
        public int w;

        /// <summary>
        /// Height
        /// </summary>
        public int h;
    }
}
