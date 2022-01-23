using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelFileManagement
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

    public LevelFileManagement(string dataPath)
    {
        _dataPath = dataPath;
    }

    private string GetPath(string folderName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var path = $"{_dataPath}{SONGSFOLDER}{folderName}/";
#elif UNITY_EDITOR
        //var dataPath = 
        var path = $"{_dataPath}{UNITYEDITORLOCATION}{folderName}/";
#endif
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
