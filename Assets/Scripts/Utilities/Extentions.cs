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
    public static Texture2D ScaleTexture(this Texture2D myTexture, int newWidth, int newHeight, TextureFormat format)
    {
        var result = new Texture2D(newWidth, newHeight, format, false)
        {
            name = myTexture.name,
        };

        var pixels = result.GetPixels(0);
        var xIncrease = (1f / myTexture.width) * ((float)myTexture.width / newWidth);
        var yIncrease = (1f / myTexture.height) * ((float)myTexture.height / newHeight);
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = myTexture.GetPixelBilinear(xIncrease * ((float)i % newWidth),
                yIncrease * (Mathf.Floor(i / newWidth)));
        }
        result.SetPixels(pixels, 0);
        result.Apply();
        myTexture = result;
        return result;
    }
    
    
    
}
