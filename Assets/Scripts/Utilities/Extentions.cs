using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using Cysharp.Threading.Tasks;

public static class Extentions
{
    public static void Shuffle<T>(this T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            var k = UnityEngine.Random.Range(0, n--);
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
            var k = UnityEngine.Random.Range(0, n--);
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
        return result;
    }
    public static async UniTask<Texture2D> ScaleTextureAsync(this Texture2D myTexture, int newWidth, int newHeight, TextureFormat format)
    {
        var result = new Texture2D(newWidth, newHeight, format, false)
        {
            name = myTexture.name,
        };
        var pixels = result.GetPixels(0);
        var width = myTexture.width;
        var height = myTexture.height;
        var xIncrease = (1f / width) * ((float)width / newWidth);
        var yIncrease = (1f / height) * ((float)height / newHeight);
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = myTexture.GetPixelBilinear(xIncrease * ((float)i % newWidth),
                yIncrease * (Mathf.Floor(i / newWidth)));
            if(i % newWidth == 0)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }
        result.SetPixels(pixels, 0);
        result.Apply();
        return result;
    }

    /*public static async UniTask<Texture2D> ScaleTextureAsync(this Texture2D myTexture, int newWidth, int newHeight, TextureFormat format)
    {
        var result = new Texture2D(newWidth, newHeight, format, false)
        {
            name = myTexture.name,
        };

        var sourcePixels = myTexture.GetPixelData<Color>(0);
        //var xIncrease = (1f / myTexture.width) * ((float)myTexture.width / newWidth);
        //var yIncrease = (1f / myTexture.height) * ((float)myTexture.height / newHeight);
        var resultPixels = result.GetPixelData<Color>(0);
        var scaleTextureJob = new ScaleTextureJob(sourcePixels, resultPixels, myTexture.width, myTexture.width, newWidth, newHeight);
        await scaleTextureJob.Schedule(resultPixels.Length, 5);

        result.SetPixelData(resultPixels, 0);
        result.Apply();
        sourcePixels.Dispose();
        resultPixels.Dispose();

        return result;
    }*/

    private struct ScaleTextureJob : IJobParallelFor
    {
        [ReadOnly]
        public readonly NativeArray<Color>.ReadOnly sourcePixels;
        public NativeArray<Color> resultPixels;
        private readonly int width;
        private readonly int height;
        private readonly int newWidth;
        private readonly int newHeight;

        public ScaleTextureJob(NativeArray<Color> sourcePixels, NativeArray<Color> resultPixels, int width, int height, int newWidth, int newHeight)
        {
            this.sourcePixels = sourcePixels.AsReadOnly();
            this.resultPixels = resultPixels;
            this.width = width;
            this.height = height;
            this.newWidth = newWidth;
            this.newHeight = newHeight;
        }

        public void Execute(int index)
        {
            int y = (int)math.ceil((float)index / width);
            var x = index / 1 + (width * y);
            resultPixels[index] = GetPixelBilinear(x, y);
        }

        private Color GetPixelBilinear(float x, float y)
        {
            var floor_x = (int)math.floor(x);
            var floor_y = (int)math.floor(y);
            var ceil_x = floor_x + 1;
            if (ceil_x >= newWidth) ceil_x = floor_x;
            var ceil_y = floor_y + 1;
            if (ceil_y >= newHeight) ceil_y = floor_y;
            var fraction_x = x - floor_x;
            var fraction_y = y - floor_y;
            var one_minus_x = 1.0 - fraction_x;
            var one_minus_y = 1.0 - fraction_y;

            var c1 = GetPixel(floor_x, floor_y);
            var c2 = GetPixel(ceil_x, floor_y);
            var c3 = GetPixel(floor_x, ceil_y);
            var c4 = GetPixel(ceil_x, ceil_y);

            // Blue
            var b1 = (byte)(one_minus_x * c1.b + fraction_x * c2.b);

            var b2 = (byte)(one_minus_x * c3.b + fraction_x * c4.b);

            var blue = (byte)(one_minus_y * (float)(b1) + fraction_y * (float)(b2));

            // Green
            b1 = (byte)(one_minus_x * c1.g + fraction_x * c2.g);

            b2 = (byte)(one_minus_x * c3.g + fraction_x * c4.g);

            var green = (byte)(one_minus_y * (float)(b1) + fraction_y * (float)(b2));

            // Red
            b1 = (byte)(one_minus_x * c1.r + fraction_x * c2.r);

            b2 = (byte)(one_minus_x * c3.r + fraction_x * c4.r);

            var red = (byte)(one_minus_y * (float)(b1) + fraction_y * (float)(b2));

            return new Color32(red, green, blue, 255);
        }

        private Color GetPixel(int x, int y)
        {
            var index = (y * (int)math.floor(width)) + x;
            return sourcePixels[index];
        }
    }
}
