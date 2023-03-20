using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

sealed class MapPacker
{
    private const string ToolLocation = "Tools/Map Packer";

    [MenuItem(ToolLocation)]
    private static void OpenWindow()
    {
        var window = EditorWindow.GetWindow<MapPackerEditor>();
    }

    private class MapPackerEditor : EditorWindow
    {
        private const string RChannel = "R Channel: ";
        private const string GChannel = "G Channel: ";
        private const string BChannel = "B Channel: ";
        private const string AChannel = "A Channel: ";
        private const string Generate = "Generate Texture";
        private const string PackedTextureName = "/PackedTexture.png";

        private Texture2D _redTextureSource;
        private Texture2D _greenTextureSource;
        private Texture2D _blueTextureSource;
        private Texture2D _alphaTextureSource;

        private void OnGUI()
        {
            _redTextureSource = (Texture2D)EditorGUILayout.ObjectField(RChannel, _redTextureSource, typeof(Texture2D), false);
            _greenTextureSource = (Texture2D)EditorGUILayout.ObjectField(GChannel, _greenTextureSource, typeof(Texture2D), false);
            _blueTextureSource = (Texture2D)EditorGUILayout.ObjectField(BChannel, _blueTextureSource, typeof(Texture2D), false);
            _alphaTextureSource = (Texture2D)EditorGUILayout.ObjectField(AChannel, _alphaTextureSource, typeof(Texture2D), false);

            if (GUILayout.Button(Generate))
            {
                //Debug.Log($"{Application.dataPath}{PackedTextureName}");
                //return;
                var newTexture = new Texture2D(_redTextureSource.width, _redTextureSource.height, TextureFormat.RGBA32, false);
                for (var i = 0; i < _redTextureSource.width; i++)
                {
                    for (var j = 0; j < _redTextureSource.height; j++)
                    {
                        var r = _redTextureSource.GetPixel(i, j).r;
                        var g = _greenTextureSource.GetPixel(i, j).g;
                        var b = _blueTextureSource.GetPixel(i, j).b;
                        var a = _alphaTextureSource.GetPixel(i, j).a;
                        newTexture.SetPixel(i, j, new Color(r, g, b, a));
                    }
                }
                newTexture.Apply(true, false);
                var bytes = newTexture.EncodeToPNG();
                var path = $"{Application.dataPath}{PackedTextureName}";
                System.IO.File.WriteAllBytes(path, bytes);
            }
        }
    }
}
