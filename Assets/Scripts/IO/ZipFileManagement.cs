using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class ZipFileManagement
{
    #region Const Strings

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string ANDROIDPATHSTART = "file://";
    private const string SONGSFOLDER = "/Resources/Songs/";
#elif UNITY_EDITOR
    private const string UNITYEDITORLOCATION = "/LocalCustomSongs/Songs/";
#endif
    private static string _dataPath = string.Empty;

    #endregion


    public static void Initialize(string dataPath)
    {
        _dataPath = dataPath;
    }

    public static void ExtractAndSaveZippedSongAsync(string folderName, byte[] songBytes)
    {
        folderName = folderName.RemoveIllegalIOCharacters();

#if UNITY_ANDROID && !UNITY_EDITOR
        var path = $"{_dataPath}{SONGSFOLDER}{folderName}/";
#elif UNITY_EDITOR
        //var dataPath = 
        var path = $"{_dataPath}{UNITYEDITORLOCATION}{folderName}/";
#endif
        using var memoryStream = new MemoryStream(songBytes);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
        archive.ExtractToDirectory(path);
    }
}