using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelFileManagement
{
    private static string _dataPath = string.Empty;
    
    public LevelFileManagement(string dataPath)
    {
        _dataPath = dataPath;
    }

    private string GetPath(string folderName)
    {
        var path = $"{_dataPath}{folderName}/";
        return path;
    }

    public bool FolderExists(string folderName)
    {
        
        return Directory.Exists(GetPath(folderName));
    }

    public void DeleteFolder(string folderName)
    {
        var path = GetPath(folderName);
        foreach (var file in Directory.GetFiles(path))
        {
            File.Delete(file);
        }
        Directory.Delete(path, true);
    }
}
