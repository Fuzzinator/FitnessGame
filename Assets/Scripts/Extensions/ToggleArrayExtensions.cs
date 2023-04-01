using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ToggleArrayExtensions
{
    public static int GetToggleID(this Toggle[] togglesArray, Toggle toggle)
    {
        var toggleID = -1;
        for (var i = 0; i < togglesArray.Length; i++)
        {
            if (togglesArray[i] == toggle)
            {
                toggleID = i;
                break;
            }
        }

        return toggleID;
    }
}
