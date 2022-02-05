using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IntExtensions
{
    public const int TWELVE = 12;
    
    public static int To12HrFormat(this int value)
    {
        if (value > TWELVE)
        {
            value -= TWELVE;
        }
        else if (value == 0)
        {
            value = TWELVE;
        }

        return value;
    }
}
