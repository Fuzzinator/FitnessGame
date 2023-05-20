using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ValidHit
{
    [field: SerializeField]
    public bool IsSwinging { get;private set; }

    [field: SerializeField]
    public bool InHitAllowance { get; private set; }

    [field: SerializeField]
    public bool FastEnough { get; private set; }

    [field: SerializeField]
    public bool GoodForm { get; private set; }

    public bool IsValidHit => IsSwinging && InHitAllowance && FastEnough && GoodForm;

    public ValidHit (bool isSwinging, bool inHitAllowance, bool fastEnough, bool goodForm)
    {
        IsSwinging = isSwinging;
        InHitAllowance = inHitAllowance;
        FastEnough = fastEnough;
        GoodForm = goodForm;
    }
}
