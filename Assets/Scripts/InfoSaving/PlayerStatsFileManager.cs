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

        private static readonly string Path = $"{AssetManager.DataPath}{DATAFOLDER}";

        private static bool _accessingSongRecords;
        private static bool _accessingPlaylistRecords;

        private static ES3Settings _songSettings;
        private static ES3Settings _playlistSettings;

        private static readonly string SongFolder = $"{Path}{SONGRECORDS}";
        private static readonly string PlaylistFolder = $"{Path}{PLAYLISTRECORDS}";

        public static async UniTask<bool> RecordSongValue<T>(string key, T value, CancellationToken token)
        {
            await UniTask.WaitWhile(() => _accessingSongRecords, cancellationToken: token);
            _accessingSongRecords = true;
            if (_songSettings == null)
            {
                _songSettings = new ES3Settings(SongFolder);
            }
            var returnValue = await RecordValue(key, value, _songSettings, token);
            _accessingSongRecords = false;
            return returnValue;
        }

        public static async UniTask<object> GetSongValue<T>(string key, CancellationToken token)
        {
            await UniTask.WaitWhile(() => _accessingSongRecords, cancellationToken: token);
            _accessingSongRecords = true;
            
            if (_songSettings == null)
            {
                _songSettings = new ES3Settings(SongFolder);
            }
            var returnValue= await GetValue<T>(key, _songSettings, token);
            _accessingSongRecords = false;
            return returnValue;
        }

        public static async UniTask<object> GetPlaylistValue<T>(string key, CancellationToken token)
        {
            await UniTask.WaitWhile(() => _accessingPlaylistRecords, cancellationToken: token);
            _accessingPlaylistRecords = true;
            
            if (_playlistSettings == null)
            {
                _playlistSettings = new ES3Settings(PlaylistFolder);
            }
            var returnValue = await GetValue<T>(key, _playlistSettings, token);
            _accessingPlaylistRecords = false;
            return returnValue;
        }

        public static async UniTask RecordPlaylistValue<T>(string key, T value, CancellationToken token)
        {
            await UniTask.WaitWhile(() => _accessingPlaylistRecords, cancellationToken: token);
            _accessingPlaylistRecords = true;
            
            if (_playlistSettings == null)
            {
                _playlistSettings = new ES3Settings(PlaylistFolder);
            }
            await RecordValue(key, value, _playlistSettings, token);
            _accessingPlaylistRecords = false;
        }

        private static async UniTask<bool> RecordValue<T>(string key, T value, string folder, CancellationToken token)
        {
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
        private static async UniTask<bool> RecordValue<T>(string key, T value, ES3Settings settings, CancellationToken token)
        {
            try
            {
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
        
        private static async UniTask<object> GetValue<T>(string key, ES3Settings settings, CancellationToken token)
        {
            try
            {
                return await UniTask.RunOnThreadPool(() => ES3.Load<T>(key, settings), cancellationToken: token);
            }
            catch (Exception e)when (e is OperationCanceledException)
            {
            }

            return null;
        }

        public static async UniTask<bool> PlaylistKeyExists(string key)
        {
            if (_playlistSettings == null)
            {
                _playlistSettings = new ES3Settings(PlaylistFolder);
            }
            return await KeyExists(key, _playlistSettings);
        }

        public static async UniTask<bool> SongKeyExists(string key)
        {
            if (_songSettings == null)
            {
                _songSettings = new ES3Settings(SongFolder);
            }
            return await KeyExists(key, _songSettings);
        }

        private static async UniTask<bool> KeyExists(string key, string folder)
        {
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
        
        private static async UniTask<bool> KeyExists(string key, ES3Settings settings)
        {
            try
            {
                return ES3.KeyExists(key, settings); //UniTask.RunOnThreadPool(() => ES3.KeyExists(key, settings));
            }
            catch (Exception e)when (e is OperationCanceledException)
            {
            }

            return false;
        }
    }
}