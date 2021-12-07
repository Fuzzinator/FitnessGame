using System.Collections.Generic;
using UnityEngine;

public static class Extentions
{
    public static void Shuffle<T>(this T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            var k = Random.Range(0,n--);
            var temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
    public static void Shuffle<T>(this List<T> array)
    {
        int n = array.Count;
        while (n > 1)
        {
            var k = Random.Range(0,n--);
            var temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
}