using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FloatExtensions
{    public static float Normalized(this float value, float min, float max)
    {
        return (value - min) / (max - min);
    }
}
