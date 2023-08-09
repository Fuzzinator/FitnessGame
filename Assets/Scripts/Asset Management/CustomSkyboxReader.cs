using Superla.RadianceHDR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CustomSkyboxReader : MonoBehaviour
{
    [SerializeField]
    private Material _mat;

    private const string SkyboxesFolder = "/LocalCustomSkyboxes/";
    private const string Png = ".png";
    private const string Jpg = ".jpg";
    private const string Exr = ".exr";
    private const string Hdr = ".hdr";
    private const int CubemapResolution = 1024;
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
    // Start is called before the first frame update
    void Start()
    {
        var images = CustomEnvironmentsController.GetImagePathsInDownloads();


        return;
        var path = $"{AssetManager.DataPath}{SkyboxesFolder}";

        var info = new DirectoryInfo(path);
        var files = info.GetFiles();
        foreach (var file in files)
        {
            if (file == null)
            {
                continue;
            }

            Texture2D source;
            if (string.Equals(file.Extension, Exr, StringComparison.InvariantCultureIgnoreCase))//string.Equals(file.Extension, Png, System.StringComparison.InvariantCultureIgnoreCase))
            {
                byte[] fileData = File.ReadAllBytes(file.FullName);
                var radianceTexture = new RadianceHDRTexture(fileData);
                source = radianceTexture.texture;
            }
            else
            {
                // Step 1: Load the PNG texture from the file
                source = LoadPanoramicTexture(file.FullName);
            }
                var cubemap = new Cubemap(CubemapResolution, source.format, false);
                SetCubeMapColors(cubemap, source);

                _mat.SetTexture("_Albedo", cubemap);
            //}
        }
    }

    public static Cubemap LoadCubemap(string path)
    {
        var source = LoadPanoramicTexture(path);

        var cubemap = new Cubemap(CubemapResolution, TextureFormat.RGB24, false);

        SetCubeMapColors(cubemap, source);

        return cubemap;
    }
    private static Texture2D LoadPanoramicTexture(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }

    private static void SetCubeMapColors(Cubemap cubemap, Texture2D sourceTexture)
    {
        Color[] CubeMapColors;

        for (int i = 0; i < 6; i++)
        {
            CubeMapColors = CreateCubemapTexture(CubemapResolution, (CubemapFace)i, sourceTexture);
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
    private static Color[] CreateCubemapTexture(int resolution, CubemapFace face, Texture2D sourceTexture)
    {
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);

        Vector3 texelX_Step = (faces[(int)face][1] - faces[(int)face][0]) / resolution;
        Vector3 texelY_Step = (faces[(int)face][3] - faces[(int)face][2]) / resolution;

        float texelSize = 1.0f / resolution;
        float texelIndex = 0.0f;

        //Create textured face
        Color[] cols = new Color[resolution];
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
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();

        Color[] colors = texture.GetPixels();
        DestroyImmediate(texture);

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

        return sourceTexture.GetPixel(texelX, sourceTexture.height - texelY - 1);
    }
}
