using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal static class ArrayExtensions
{
    internal static float Average(this float[] enumerable)
    {
        int length = enumerable.Length;

        if (length == 0)
        {
            throw new InvalidOperationException("Cannot compute the average of an empty array.");
        }

        float sum = 0f;
        for (int i = 0; i < length; i++)
        {
            sum += enumerable[i];
        }

        return sum / length;
    }
}
