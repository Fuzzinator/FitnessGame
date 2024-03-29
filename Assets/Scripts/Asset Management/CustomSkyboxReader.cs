using Cysharp.Threading.Tasks;
using Superla.RadianceHDR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public static class CustomSkyboxReader
{
    private const string SkyboxesFolder = "/LocalCustomSkyboxes/";
    private const string Png = ".png";
    private const string Jpg = ".jpg";
    private const string Exr = ".exr";
    private const string Hdr = ".hdr";
    private const int CubemapResolution = 1024;

    //private static bool _waitingForCubemap = false;
    private static Cubemap _customSkybox = null;
    private static string _loadedSkybox = null;

    /// <summary>
    /// These are the faces of a cube
    /// </summary>
    private static Vector3[][] faces =
    {
        new Vector3[] {
            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, 1.0f)
        },
        new Vector3[] {
            new Vector3(-1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(-1.0f, -1.0f, -1.0f)
        },
        new Vector3[] {
            new Vector3(-1.0f, 1.0f, 1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, -1.0f)
        },
        new Vector3[] {
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, 1.0f)
        },
        new Vector3[] {
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f)
        },
        new Vector3[] {
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, 1.0f),
            new Vector3(-1.0f, -1.0f, 1.0f)
        }
    };

    public static async UniTask<Cubemap> GetCubeMap(string path)
    {
        if(string.IsNullOrWhiteSpace(path))
        {
            return null;
        }
        if(string.Equals(_loadedSkybox, path, StringComparison.InvariantCultureIgnoreCase) && _customSkybox != null)
        {
            return _customSkybox;
        }
        return await LoadCubemap(path);
    }

    public static async UniTask<Cubemap> LoadCubemap(string path)
    {
        var source = await LoadPanoramicTexture(path);

        var cubemap = new Cubemap(CubemapResolution, TextureFormat.RGB24, false);

        await SetCubeMapColors(cubemap, source);

        return cubemap;
    }
    private static async UniTask<Texture2D> LoadPanoramicTexture(string path)
    {
        byte[] fileData = await File.ReadAllBytesAsync(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }

    private static async UniTask SetCubeMapColors(Cubemap cubemap, Texture2D sourceTexture)
    {
        Color[] CubeMapColors;

        for (int i = 0; i < 6; i++)
        {
            CubeMapColors = await CreateCubemapTexture(CubemapResolution, (CubemapFace)i, sourceTexture);
            cubemap.SetPixels(CubeMapColors, (CubemapFace)i);
        }
        // we set the cubemap from the texture pixel by pixel
        cubemap.Apply();
    }

    /// <summary>
    /// Generates a Texture that represents the given face for the cubemap.
    /// </summary>
    /// <param name="resolution">The targetresolution in pixels</param>
    /// <param name="face">The target face</param>
    /// <returns></returns>
    private static async UniTask<Color[]> CreateCubemapTexture(int resolution, CubemapFace face, Texture2D sourceTexture)
    {
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);

        Vector3 texelX_Step = (faces[(int)face][1] - faces[(int)face][0]) / resolution;
        Vector3 texelY_Step = (faces[(int)face][3] - faces[(int)face][2]) / resolution;

        float texelSize = 1.0f / resolution;
        float texelIndex = 0.0f;

        //Create textured face
        Color[] cols = new Color[resolution];
        var index = 0;
        for (int y = 0; y < resolution; y++)
        {
            Vector3 texelX = faces[(int)face][0];
            Vector3 texelY = faces[(int)face][2];
            for (int x = 0; x < resolution; x++)
            {
                cols[x] = Project(Vector3.Lerp(texelX, texelY, texelIndex).normalized, sourceTexture);
                texelX += texelX_Step;
                texelY += texelY_Step;
            }
            texture.SetPixels(0, y, resolution, 1, cols);
            texelIndex += texelSize;
            index++;
            if(index > 64)
            {
                index = 0;
                await UniTask.DelayFrame(1);
            }
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();

        Color[] colors = texture.GetPixels();

        return colors;
    }

    /// <summary>
    /// Projects a directional vector to the texture using spherical mapping
    /// </summary>
    /// <param name="direction">The direction in which you view</param>
    /// <returns></returns>
    private static Color Project(Vector3 direction, Texture2D sourceTexture)
    {
        float theta = Mathf.Atan2(direction.z, direction.x) + Mathf.PI / 180.0f;
        float phi = Mathf.Acos(direction.y);

        int texelX = (int)(((theta / Mathf.PI) * 0.5f + 0.5f) * sourceTexture.width);
        if (texelX < 0) texelX = 0;
        if (texelX >= sourceTexture.width) texelX = sourceTexture.width - 1;
        int texelY = (int)((phi / Mathf.PI) * sourceTexture.height);
        if (texelY < 0) texelY = 0;
        if (texelY >= sourceTexture.height) texelY = sourceTexture.height - 1;
        var pixels = sourceTexture.GetRawTextureData<Color>();
        return sourceTexture.GetPixel(texelX, sourceTexture.height - texelY - 1);
    }
}
