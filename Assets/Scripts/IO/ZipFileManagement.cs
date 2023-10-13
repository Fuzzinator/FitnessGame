using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class ZipFileManagement
{
    private static string _dataPath = string.Empty;

    public static void Initialize(string dataPath)
    {
        _dataPath = dataPath;
    }

    public static void ExtractAndSaveZippedSong(string folderName, byte[] songBytes)
    {
        folderName = folderName.RemoveIllegalIOCharacters();

        var path = $"{_dataPath}{folderName}";
        using var memoryStream = new MemoryStream(songBytes);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
        archive.ExtractToDirectory(path);
    }
}