using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateCustomMipMaps
{
    private const string TOOLLOCATION = "Tools/Create Custom MipMaps";

    [MenuItem(TOOLLOCATION)]
    private static void OpenWindow()
    {
        var window = EditorWindow.GetWindow(typeof(CreateMipMapsEditor));
    }

    private class CreateMipMapsEditor : EditorWindow
    {
        private const string LISTLENGTH = "Mip Map Levels: ";
        private const string MIPMAPNAME = "Mip Map";
        private const string GENERATE = "Generate Texture";
        private bool _useArray = false;
        private List<Texture2D> _mipMapLevels = new List<Texture2D>();
        private List<Texture2DArray> _arrayMipMapLevels = new List<Texture2DArray>();

        private void OnGUI()
        {
            _useArray = EditorGUILayout.Toggle("Craete 2D Array", _useArray);
            
            if (!_useArray)
            {
                var targetCount = EditorGUILayout.IntField(LISTLENGTH, _mipMapLevels.Count);
                SetListLength(_mipMapLevels, targetCount);
                for (var i = 0; i < _mipMapLevels.Count; i++)
                {
                    _mipMapLevels[i] = (Texture2D)EditorGUILayout.ObjectField($"{MIPMAPNAME} {i}: ", _mipMapLevels[i],
                        typeof(Texture2D), false);
                }

                EditorGUI.BeginDisabledGroup(_mipMapLevels.Count == 0 || _mipMapLevels.Contains(null));
                if (GUILayout.Button(GENERATE))
                {
                    var tex0 = _mipMapLevels[0];
                    var newTexture = new Texture2D(tex0.width, tex0.height, TextureFormat.RGBA32, _mipMapLevels.Count, false);
                    for (var i = 0; i < _mipMapLevels.Count; i++)
                    {
                        newTexture.SetPixels(_mipMapLevels[i].GetPixels(0), i);
                    }
                    newTexture.alphaIsTransparency = true;
                    newTexture.wrapMode = TextureWrapMode.Repeat;
                    newTexture.filterMode = FilterMode.Trilinear;
                    newTexture.Apply(false, false);
                    //var bytes = newTexture.EncodeToPNG(); 
                    //var path = $"{Application.dataPath}/{tex0.name}.png";
                    //System.IO.File.WriteAllBytes(path, bytes);
                    AssetDatabase.CreateAsset(newTexture, $"Assets/{tex0.name}.asset");
                }
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                var targetCount = EditorGUILayout.IntField(LISTLENGTH, _arrayMipMapLevels.Count);
                SetListLength(_arrayMipMapLevels, targetCount);
                for (var i = 0; i < _arrayMipMapLevels.Count; i++)
                {
                    _arrayMipMapLevels[i] = (Texture2DArray)EditorGUILayout.ObjectField($"{MIPMAPNAME} {i}: ", _arrayMipMapLevels[i],
                        typeof(Texture2DArray), false);
                }

                EditorGUI.BeginDisabledGroup(_arrayMipMapLevels.Count == 0 || _arrayMipMapLevels.Contains(null));
                if (GUILayout.Button($"{GENERATE} 2D Array"))
                {
                    var tex0 = _arrayMipMapLevels[0];
                    var newTexture = new Texture2DArray(tex0.width, tex0.height, 4,  TextureFormat.RGBA32, _arrayMipMapLevels.Count, false);
                    for (var i = 0; i < _arrayMipMapLevels.Count; i++)
                    {
                        for (var j = 0; j < 4; j++)
                        {
                            newTexture.SetPixels(_arrayMipMapLevels[i].GetPixels(j, 0), j, i);
                        }                        
                    }
                    //newTexture.alphaIsTransparency = true;
                    newTexture.wrapMode = TextureWrapMode.Repeat;
                    newTexture.filterMode = FilterMode.Trilinear;
                    newTexture.Apply(false, false);
                    //var bytes = newTexture.EncodeToPNG(); 
                    //var path = $"{Application.dataPath}/{tex0.name}.png";
                    //System.IO.File.WriteAllBytes(path, bytes);
                    AssetDatabase.CreateAsset(newTexture, $"Assets/{tex0.name}.asset");
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        private static void SetListLength<T>(List<T> targetList, int targetLength) where T : class
        {
            while (targetLength > targetList.Count)
            {
                targetList.Add(null);
            }

            while (targetLength < targetList.Count)
            {
                targetList.RemoveAt(targetList.Count - 1);
            }
        }
    }
}