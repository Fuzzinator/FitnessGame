using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace InfoSaving
{
    public static class PlayerStatsFileManager
    {
        #region Const Strings

#if UNITY_EDITOR
        private const string UNITYEDITORLOCATION = "/PlayerStats/";
#endif

        private const string DATAFOLDER = "/Resources/PlayerStats/";
        private const string PLAYLISTRECORDS = "PlaylistRecords.txt";
        private const string SONGRECORDS = "SongRecords.txt";

        #endregion

        private static string Path
        {
            get{
#if UNITY_ANDROID && !UNITY_EDITOR
            var path = $"{Application.persistentDataPath}{DATAFOLDER}";
#elif UNITY_EDITOR
                var dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
                var path = $"{dataPath}{UNITYEDITORLOCATION}";
#endif
                return path;
            }
        }

        public static async UniTask<bool> RecordSongValue<T>(string key, T value, CancellationToken token)
        {
            var folder = $"{Path}{SONGRECORDS}";
            return await RecordValue(key, value, folder, token);
        }

        public static async UniTask<object> GetSongValue<T>(string key, CancellationToken token)
        {
            var folder = $"{Path}{SONGRECORDS}";
            return await GetValue<T>(key, folder, token);
        }

        public static async UniTask<object> GetPlaylistValue<T>(string key, CancellationToken token)
        {
            var folder = $"{Path}{PLAYLISTRECORDS}";
            return await GetValue<T>(key, folder, token);
        }
    
        public static async UniTask RecordPlaylistValue<T>(string key, T value, CancellationToken token)
        {
            var folder = $"{Path}{PLAYLISTRECORDS}";
            await RecordValue(key, value, folder, token);
        }

        private static async UniTask<bool> RecordValue<T>(string key, T value, string folder, CancellationToken token)
        {
            try
            {
                var settings = new ES3Settings(folder);
                await UniTask.Run(() => ES3.Save(key, value, settings), cancellationToken:token);
                return true;
            }
            catch (Exception e)when (e is OperationCanceledException)
            {
            }
            return false;
        }

        private static async UniTask<object> GetValue<T>(string key, string folder, CancellationToken token)
        {
            try
            {
                var settings = new ES3Settings(folder);
                return await UniTask.Run(() =>ES3.Load<T>(key, settings), cancellationToken: token);
            }
            catch (Exception e)when (e is OperationCanceledException)
            {
            }

            return null;
        }

        public static bool PlaylistKeyExists(string key)
        {
            var folder = $"{Path}{PLAYLISTRECORDS}";
            return ES3.KeyExists(key, folder);
        }
        
        public static bool SongKeyExists(string key)
        {
            var folder = $"{Path}{SONGRECORDS}";
            return ES3.KeyExists(key, folder);
        }
    }
}