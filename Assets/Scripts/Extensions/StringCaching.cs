using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringCaching
{
    private static string[] CachedInts { get; set; }
    private static string[] Seconds { get; set; }

    private const string SECONDSFORMAT = "00";

    static StringCaching()
    {
        /*Seconds = new string[60];
        for (var i = 0; i < Seconds.Length; i++)
        {
            Seconds[i] = i.ToString(SECONDSFORMAT);
        }

        CachedInts = new string[2000];
        for (var time = 0; time < CachedInts.Length; time++)
        {
            CachedInts[time] = time.ToString();
        }*/
    }

    public static string GetCachedSecondsString(this int value)
    {
        if (value >= Seconds.Length)
        {
            Debug.LogError($"Requesting {value} in seconds is invalid. Returning null");
            return null;
        }

        return Seconds[value];
    }

    public static string TryGetCachedIntString(this int value)
    {
        return value >= CachedInts.Length || value < 0 ? value.ToString() : CachedInts[value];
    }
}