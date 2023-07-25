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
    // Start is called before the first frame update
    void Start()
    {
        var path = $"{AssetManager.DataPath}{SkyboxesFolder}";

        var info = new DirectoryInfo(path);
        var files = info.GetFiles();
        foreach (var file in files)
        {
            if (file == null)
            {
                continue;
            }

            if (string.Equals(file.Extension, Png, System.StringComparison.InvariantCultureIgnoreCase))
            {
                var data = File.ReadAllBytes(file.FullName);
                var cubemap = new Cubemap(1, TextureFormat.RGB24, false);
                cubemap.SetPixelData(data, 0, CubemapFace.Unknown);
                _mat.SetTexture("_Albedo", cubemap);
            }
        }
    }
}
