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

        private static string _path;
        private static string _songFolder;
        private static string _playlistFolder;

        private static string Path
        {
            get
            {
                if (_path == null)
                {
#if UNITY_ANDROID && !UNITY_EDITOR
                    _path = $"{Application.persistentDataPath}{DATAFOLDER}";
#elif UNITY_EDITOR
                    var dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
                    _path = $"{dataPath}{UNITYEDITORLOCATION}";
#endif
                }

                return _path;
            }
        }

        private static string SongFolder
        {
            get
            {
                if (_songFolder == null)
                {
                    _songFolder = $"{Path}{SONGRECORDS}";
                }

                return _songFolder;
            }
        }

        private static string PlaylistFolder
        {
            get
            {
                if (_songFolder == null)
                {
                    _songFolder = $"{Path}{PLAYLISTRECORDS}";
                }

                return _songFolder;
            }
        }

        private static void EnsurePath()
        {
            var path = Path;
        }

        public static async UniTask<bool> RecordSongValue<T>(string key, T value, CancellationToken token)
        {
            EnsurePath();
            return await RecordValue(key, value, SongFolder, token);
        }

        public static async UniTask<object> GetSongValue<T>(string key, CancellationToken token)
        {
            EnsurePath();
            return await GetValue<T>(key, SongFolder, token);
        }

        public static async UniTask<object> GetPlaylistValue<T>(string key, CancellationToken token)
        {
            EnsurePath();
            return await GetValue<T>(key, PlaylistFolder, token);
        }

        public static async UniTask RecordPlaylistValue<T>(string key, T value, CancellationToken token)
        {
            EnsurePath();
            await RecordValue(key, value, PlaylistFolder, token);
        }

        private static async UniTask<bool> RecordValue<T>(string key, T value, string folder, CancellationToken token)
        {
            EnsurePath();
            try
            {
                var settings = new ES3Settings(folder);
                await UniTask.RunOnThreadPool(() => ES3.Save(key, value, settings), cancellationToken: token);
                return true;
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    return false;
                }
                
                Debug.LogError(e);
                return false;
            }
        }

        private static async UniTask<object> GetValue<T>(string key, string folder, CancellationToken token)
        {
            EnsurePath();
            try
            {
                var settings = new ES3Settings(folder);
                return await UniTask.RunOnThreadPool(() => ES3.Load<T>(key, settings), cancellationToken: token);
            }
            catch (Exception e)when (e is OperationCanceledException)
            {
            }

            return null;
        }

        public static async UniTask<bool> PlaylistKeyExists(string key)
        {
            return await KeyExists(key, PlaylistFolder);
        }

        public static async UniTask<bool> SongKeyExists(string key)
        {
            return await KeyExists(key, SongFolder);
        }

        private static async UniTask<bool> KeyExists(string key, string folder)
        {
            EnsurePath();
            try
            {
                var settings = new ES3Settings(folder);
                return ES3.KeyExists(key, settings); //UniTask.RunOnThreadPool(() => ES3.KeyExists(key, settings));
            }
            catch (Exception e)when (e is OperationCanceledException)
            {
            }

            return false;
        }
    }
}