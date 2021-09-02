using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ChoreographyEvent : ISequenceable//First release wont use this probably
{
    public float Time => _time;
    public EventType Type => _type;
    public EventValue Value => _value;
    
    [SerializeField]
    private float _time;
    [SerializeField]
    private EventType _type;
    [SerializeField]
    private EventValue _value;


    public HitSideType HitSideType
    {
        get { return HitSideType.Unused; }
    }
    public enum EventType
    {
        BackLasers = 0,
        RingLights = 1,
        LeftRotatingLasers = 2,
        RightRotatingLasers = 3,
        CenterLights = 4,
        BoostLightSecondaryColors = 5,
        ExtraLeftSideLights = 6,
        ExtraRightSideLights = 7,
        CreateOneRingSpin = 8,
        RingZoom = 9,
        BPMChanges = 10,
        Unused = 11,
        LeftRotatingLaserSpeed = 12,
        RightRotatingLaserSpeed = 13,
        EarlyRotation = 14,//for 360/90 mode
        LateRotation = 15,//for 360/90 mode
        LowerCarHydrolics = 16,//wtf?
        RaiseCarHydrolics = 17
    }

    public enum EventValue
    {
        LightOff = 0,
        LightOnRight = 1,
        FlashLightToNormalRight = 2,
        FlashLightToBlackRight = 3,
        Unused = 4,
        LightOnLeft = 5,
        FlashLightToNormalLeft = 6,
        FlashLightToBlackLeft = 7,
    }
}
