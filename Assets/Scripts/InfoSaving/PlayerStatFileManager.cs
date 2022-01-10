using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

public class PlayerStatFileManager : MonoBehaviour
{
    public string test = "tewst";
    #region Const Strings

#if UNITY_EDITOR
    private const string UNITYEDITORLOCATION = "/PlayerStats/";
#endif

    private const string DATAFOLDER = "/Resources/PlayerStats/";
    private const string PLAYLISTRECORDS = "PlaylistRecords";
    private const string SONGRECORDS = "SongRecords";
    private const string TXT = ".txt";

    #endregion

    public async void RecordValue<T>(string key, T value, bool songRecord, CancellationToken token)
        where T : Object
    {
#if UNITY_ANDROID && !UNITY_EDITOR
            var path = $"{Application.persistentDataPath}{DATAFOLDER}";
#elif UNITY_EDITOR
        var dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
        var path = $"{dataPath}{UNITYEDITORLOCATION}";
#endif

        try
        {
            var folder = $"{DATAFOLDER}{(songRecord ? SONGRECORDS : PLAYLISTRECORDS)}";
            ES3.Save(key, JsonUtility.ToJson(value), folder);
        }
        catch (Exception e)when (e is OperationCanceledException)
        {
        }
    }
}