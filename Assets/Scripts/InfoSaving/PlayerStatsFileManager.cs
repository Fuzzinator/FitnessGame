using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameModeManagement;
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

        private const string STREAK = "Streak:";
        private const string SCORE = "Score:";

        #endregion

        private static readonly string Path = $"{AssetManager.DataPath}{DATAFOLDER}";

        private static bool _accessingSongRecords;
        private static bool _accessingPlaylistRecords;

        private static ES3Settings _songSettings;
        private static ES3Settings _playlistSettings;


        private static readonly string SongFolder = $"{Path}{SONGRECORDS}";
        private static readonly string PlaylistFolder = $"{Path}{PLAYLISTRECORDS}";


        private static ES3Settings SongSettings => _songSettings ??= new ES3Settings(SongFolder);
        private static ES3Settings PlaylistSettings => _playlistSettings ??= new ES3Settings(PlaylistFolder);


        public static async UniTask<bool> RecordSongValue<T>(string key, T value, CancellationToken token)
        {
            await UniTask.WaitWhile(() => _accessingSongRecords, cancellationToken: token);
            _accessingSongRecords = true;

            var returnValue = await RecordValue(key, value, SongSettings, token);
            _accessingSongRecords = false;
            return returnValue;
        }

        public static async UniTask<object> GetSongValue<T>(string key, CancellationToken token)
        {
            await UniTask.WaitWhile(() => _accessingSongRecords, cancellationToken: token);
            _accessingSongRecords = true;

            var returnValue = await GetValue<T>(key, SongSettings, token);
            _accessingSongRecords = false;
            return returnValue;
        }

        public static async UniTask<object> GetPlaylistValue<T>(string key, CancellationToken token)
        {
            await UniTask.WaitWhile(() => _accessingPlaylistRecords, cancellationToken: token);
            _accessingPlaylistRecords = true;

            var returnValue = await GetValue<T>(key, PlaylistSettings, token);
            _accessingPlaylistRecords = false;
            return returnValue;
        }

        public static async UniTask RecordPlaylistValue<T>(string key, T value, CancellationToken token)
        {
            await UniTask.WaitWhile(() => _accessingPlaylistRecords, cancellationToken: token);
            _accessingPlaylistRecords = true;

            await RecordValue(key, value, PlaylistSettings, token);
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

        private static async UniTask<bool> RecordValue<T>(string key, T value, ES3Settings settings,
            CancellationToken token)
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
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n {e.StackTrace}");
            }

            return null;
        }

        public static async UniTask<bool> PlaylistKeyExists(string key)
        {
            return await KeyExists(key, PlaylistSettings);
        }

        public static async UniTask<bool> SongKeyExists(string key)
        {
            return await KeyExists(key, SongSettings);
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

        public static void DeleteSongKey(string key)
        {
            ES3.DeleteKey(key, SongFolder);
        }


        public static void DeletePlaylistKey(string key)
        {
            ES3.DeleteKey(key, PlaylistFolder);
        }

        public static async UniTask<SongAndPlaylistRecords> TryGetRecords(SongInfo info, DifficultyInfo.DifficultyEnum difficultyEnum,
            GameMode gameMode, SongAndPlaylistScoreRecord[] records, SongAndPlaylistStreakRecord[] streaks,
            CancellationToken token)
        {
            var currentSongScoreName = SongInfoReader.GetFullSongName(info, difficultyEnum, gameMode,  SCORE);
            var hasScoreRecord = await SongKeyExists(currentSongScoreName);

            var currentSongStreakName = SongInfoReader.GetFullSongName(info, difficultyEnum, gameMode , STREAK);
            var hasStreakRecord = await SongKeyExists(currentSongStreakName);

            var exists = hasScoreRecord && hasStreakRecord;

            if (!exists)
            {
                var oldKey = PlaylistManager.Instance.GetFullSongName();
                exists = await SongKeyExists(oldKey);
                if (exists)
                {
                    await UpgradeFromSingleStatsRecord(records, streaks, oldKey, token);
                    DeleteSongKey(oldKey);
                }
            }
            else
            {
                try
                {
                    records =
                        (SongAndPlaylistScoreRecord[]) await GetSongValue<SongAndPlaylistScoreRecord[]>(
                            currentSongScoreName,
                            token);
                    streaks =
                        (SongAndPlaylistStreakRecord[]) await GetSongValue<SongAndPlaylistStreakRecord[]>(
                            currentSongStreakName,
                            token);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw;
                }
            }

            return new SongAndPlaylistRecords(exists, records, streaks);
        }

        

        #region Upgrading

        public static async UniTask UpgradeFromSingleStatsRecord(SongAndPlaylistScoreRecord[] scores,
            SongAndPlaylistStreakRecord[] streaks, string key, CancellationToken token)
        {
            var oldRecord = (SongAndPlaylistRecord) await GetSongValue<SongAndPlaylistRecord>(
                key, token);
            oldRecord = new SongAndPlaylistRecord(oldRecord.Score, oldRecord.Streak);
            scores[0] = new SongAndPlaylistScoreRecord(oldRecord.Score);
            streaks[0] = new SongAndPlaylistStreakRecord(oldRecord.Streak);
            for (var i = 1; i < scores.Length; i++)
            {
                scores[i] = new SongAndPlaylistScoreRecord();
                streaks[i] = new SongAndPlaylistStreakRecord();
            }
        }

        #endregion
    }
}