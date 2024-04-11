using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameModeManagement;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SocialPlatforms.Impl;
using static Cinemachine.CinemachineTriggerAction.ActionSettings;
using static DifficultyInfo;
using static UnityEngine.XR.Hands.XRHandSubsystemDescriptor;

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


        private static readonly string SongFolder = $"{Path}{SONGRECORDS}";//"D:\\Projects\\Shadow BoXR Oculus Build\\Shadow BoXR_Data\\Resources\\PlayerStats\\SongRecords.txt";
        private static readonly string PlaylistFolder = $"{Path}{PLAYLISTRECORDS}";

        private static readonly PlaylistRecord[] _playlistScoreAndStreakRecord = new PlaylistRecord[5];
        private static readonly SongRecord[] _songScoreAndStreakRecord = new SongRecord[10];

        private static ES3Settings SongSettings => _songSettings ??= new ES3Settings(SongFolder);
        private static ES3Settings PlaylistSettings => _playlistSettings ??= new ES3Settings(PlaylistFolder);


        public static async UniTask<bool> RecordSongValue<T>(string key, T value, CancellationToken token)
        {
            await UniTask.WaitWhile(() => _accessingSongRecords, cancellationToken: token);
            _accessingSongRecords = true;

            var returnValue = await SetValue(key, value, SongSettings, token);
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

            await SetValue(key, value, PlaylistSettings, token);
            _accessingPlaylistRecords = false;
        }

        private static async UniTask<bool> SetValue<T>(string key, T value, string folder, CancellationToken token)
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

        private static async UniTask<bool> SetValue<T>(string key, T value, ES3Settings settings,
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
                else if (e is InvalidCastException)
                {

                    AzureSqlManager.Instance.TrySendErrorReport($"Encountered error setting setting \"{key}\" reported error is: {e.Message}", e.StackTrace);
                    ES3.DeleteKey(key, settings);
                    await SetValue(key, value, settings, token);
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
            catch (Exception e) when (e is OperationCanceledException)
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
            catch (Exception e) when (e is OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n {e.StackTrace}");
            }

            return null;
        }

        public static async UniTask<bool> PlaylistKeyExists(string key, CancellationToken token)
        {
            await UniTask.WaitWhile(() => _accessingPlaylistRecords, cancellationToken: token);
            _accessingPlaylistRecords = true;
            var keyExists = KeyExists(key, PlaylistSettings);
            _accessingPlaylistRecords = false;
            return keyExists;
        }

        public static async UniTask<bool> SongKeyExists(string key, CancellationToken token)
        {
            await UniTask.WaitWhile(() => _accessingSongRecords, cancellationToken: token);
            _accessingSongRecords = true;
            var keyExists = KeyExists(key, SongSettings);
            _accessingSongRecords = false;
            return keyExists;
        }

        private static bool KeyExists(string key, string folder)
        {
            try
            {
                var settings = new ES3Settings(folder);
                return ES3.KeyExists(key, settings); //UniTask.RunOnThreadPool(() => ES3.KeyExists(key, settings));
            }
            catch (Exception e) when (e is OperationCanceledException)
            {
            }

            return false;
        }

        private static bool KeyExists(string key, ES3Settings settings)
        {
            try
            {
                return ES3.KeyExists(key, settings); //UniTask.RunOnThreadPool(() => ES3.KeyExists(key, settings));
            }
            catch (Exception e) when (e is OperationCanceledException)
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

        public static async UniTask<SongRecord[]> TryGetRecords(SongInfo info, DifficultyEnum difficultyEnum,
            GameMode gameMode, CancellationToken token)
        {
            if (!string.IsNullOrWhiteSpace(info.SongID))
            {
                var currentSongName = SongInfoReader.GetFullSongName(info, difficultyEnum, gameMode);
                var hasRecord = await SongKeyExists(currentSongName, token);
                if (hasRecord)
                {
                    try
                    {
                        var recordScores =
                            (SongRecord[])await GetSongValue<SongRecord[]>(
                                currentSongName, token) ?? _songScoreAndStreakRecord;

                        return recordScores;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        throw;
                    }
                }
                else
                {
                    var oldRecord = await TryGetOldSongRecords(info, difficultyEnum, gameMode, token);
                    if (oldRecord.hasRecord)
                    {
                        ConvertOldSongRecords(oldRecord, _songScoreAndStreakRecord);
                        RecordSongValue(currentSongName, _songScoreAndStreakRecord, token).Forget();
                        return _songScoreAndStreakRecord;
                    }
                }
            }
            else
            {
                var currentSongName = SongInfoReader.GetFullSongNameNoID(info, difficultyEnum, gameMode);
                var hasRecord = await SongKeyExists(currentSongName, token);
                if (hasRecord)
                {
                    try
                    {
                        var recordScores =
                            (SongRecord[])await GetSongValue<SongRecord[]>(
                                currentSongName, token) ?? _songScoreAndStreakRecord;

                        return recordScores;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        throw;
                    }
                }
                else
                {
                    var noIDRecord = await GetOldRecordFromName(info, difficultyEnum, gameMode, token, false);
                    if (noIDRecord.hasRecord)
                    {
                        ConvertOldSongRecords(noIDRecord, _songScoreAndStreakRecord);
                        return _songScoreAndStreakRecord;
                    }
                }
            }

            ClearSongRecords();
            return _songScoreAndStreakRecord;
        }

        public static async UniTask<SongAndPlaylistRecords> TryGetOldSongRecords(SongInfo info, DifficultyEnum difficultyEnum,
           GameMode gameMode, CancellationToken token, bool deleteOldRecord = false)
        {
            if (!string.IsNullOrWhiteSpace(info.SongID))
            {
                var currentSongScoreName = SongInfoReader.GetFullSongName(info, difficultyEnum, gameMode, SCORE);
                var hasScoreRecord = await SongKeyExists(currentSongScoreName, token);

                var currentSongStreakName = SongInfoReader.GetFullSongName(info, difficultyEnum, gameMode, STREAK);
                var hasStreakRecord =await  SongKeyExists(currentSongStreakName, token);

                if (!hasStreakRecord || !hasScoreRecord)
                {
                    var records = await GetOldRecordFromName(info, difficultyEnum, gameMode, token, false);//TODO update songs
                    if (records.hasRecord)
                    {
                        return records;
                    }
                }

                if (hasScoreRecord && hasStreakRecord)
                {
                    try
                    {
                        var recordScores =
                            (SongAndPlaylistScoreRecord[])await GetSongValue<SongAndPlaylistScoreRecord[]>(
                                currentSongScoreName, token) ?? new SongAndPlaylistScoreRecord[5];


                        var recordStreaks =
                            (SongAndPlaylistStreakRecord[])await GetSongValue<SongAndPlaylistStreakRecord[]>(
                                currentSongStreakName, token) ?? new SongAndPlaylistStreakRecord[5];
                        await UniTask.DelayFrame(1);
                        if (deleteOldRecord)
                        {
                            DeleteSongKey(currentSongScoreName);
                            DeleteSongKey(currentSongStreakName);
                        }
                        return new SongAndPlaylistRecords(true, recordScores, recordStreaks);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        throw;
                    }
                }

            }

            var oldRecords = await GetOldRecordFromName(info, difficultyEnum, gameMode, token, false);//TODO update songs
            if (oldRecords.hasRecord)
            {
                return oldRecords;
            }

            return new SongAndPlaylistRecords();
        }

        private static void ConvertOldSongRecords(SongAndPlaylistRecords oldRecord, SongRecord[] newRecord)
        {
            var streaks = ListPool<SongAndPlaylistStreakRecord>.Get();
            streaks.AddRange(oldRecord.streaks);
            for (var i = 0; i < oldRecord.scores.Length; i++)
            {
                var score = oldRecord.scores[i];
                if (!score.IsValid)
                {
                    continue;
                }
                var streak = 0;
                for (var j = 0; j < streaks.Count; j++)
                {
                    if (string.Equals(streaks[j].Guid, score.Guid))
                    {
                        streak = streaks[j].Streak;
                        streaks.RemoveAt(j);
                        break;
                    }
                }

                newRecord[i] = new SongRecord(score.ProfileName, score.Guid, (int)score.Score, streak);
            }
            ListPool<SongAndPlaylistStreakRecord>.Release(streaks);
        }
        private static void ConvertOldPlaylistRecords(SongAndPlaylistRecords oldRecord, PlaylistRecord[] newRecord)
        {
            var streaks = ListPool<SongAndPlaylistStreakRecord>.Get();
            streaks.AddRange(oldRecord.streaks);
            for (var i = 0; i < oldRecord.scores.Length; i++)
            {
                var score = oldRecord.scores[i];
                if (!score.IsValid)
                {
                    continue;
                }
                var streak = 0;
                for (var j = 0; j < streaks.Count; j++)
                {
                    if (string.Equals(streaks[j].Guid, score.Guid))
                    {
                        streak = streaks[j].Streak;
                        streaks.RemoveAt(j);
                        break;
                    }
                }

                newRecord[i] = new PlaylistRecord(score.ProfileName, score.Guid, score.Score, streak);
            }
            ListPool<SongAndPlaylistStreakRecord>.Release(streaks);
        }

        private static void ClearSongRecords()
        {
            for (var i = 0; i < _songScoreAndStreakRecord.Length; i++)
            {
                _songScoreAndStreakRecord[i] = new SongRecord();
            }
        }
        private static void ClearPlaylistRecords()
        {
            for (var i = 0; i < _playlistScoreAndStreakRecord.Length; i++)
            {
                _playlistScoreAndStreakRecord[i] = new PlaylistRecord();
            }
        }

        public static async UniTask<PlaylistRecord[]> TryGetPlaylistRecords(Playlist playlist, CancellationToken token)
        {
            var playlistFullScoreName = $"{playlist.GUID}-{playlist.DifficultyEnum}-{playlist.TargetGameMode}";
            
            var scoreExists = await PlaylistKeyExists(playlistFullScoreName, token);

            if (scoreExists)
            {
                try
                {

                    var recordScores =
                        (PlaylistRecord[])await GetPlaylistValue<PlaylistRecord[]>(
                            playlistFullScoreName, token) ?? new PlaylistRecord[5];
                    return recordScores;

                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw;
                }

            }
            else
            {
                var oldRecords = await TryGetOldPlaylistRecords(playlist, token);
                if (oldRecords.hasRecord)
                {
                    ConvertOldPlaylistRecords(oldRecords, _playlistScoreAndStreakRecord);
                    RecordPlaylistValue(playlistFullScoreName, _playlistScoreAndStreakRecord, token).Forget();
                    return _playlistScoreAndStreakRecord;
                }
            }

            ClearPlaylistRecords();
            return _playlistScoreAndStreakRecord;
        }

        public static async UniTask<SongAndPlaylistRecords> TryGetOldPlaylistRecords(Playlist playlist, CancellationToken token)
        {
            var playlistFullScoreName = $"{SCORE}{playlist.GUID}-{playlist.DifficultyEnum}-{playlist.TargetGameMode}";
            var playlistFullStreakName = $"{STREAK}{playlist.GUID}-{playlist.DifficultyEnum}-{playlist.TargetGameMode}";
            
            var scoreExists = await PlaylistKeyExists(playlistFullScoreName, token);
            var streakExists = await PlaylistKeyExists(playlistFullStreakName, token);
            var exists = scoreExists && streakExists;

            if (exists)
            {
                try
                {
                    var recordScores =
                        (SongAndPlaylistScoreRecord[])await GetPlaylistValue<SongAndPlaylistScoreRecord[]>(
                            playlistFullScoreName, token) ?? new SongAndPlaylistScoreRecord[5];


                    var recordStreaks =
                        (SongAndPlaylistStreakRecord[])await GetPlaylistValue<SongAndPlaylistStreakRecord[]>(
                            playlistFullStreakName, token) ?? new SongAndPlaylistStreakRecord[5];

                    return new SongAndPlaylistRecords(true, recordScores, recordStreaks);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw;
                }
            }

            return new SongAndPlaylistRecords(false, new SongAndPlaylistScoreRecord[5], new SongAndPlaylistStreakRecord[5]);
        }


        #region Upgrading

        private static async UniTask<SongAndPlaylistRecords> GetOldRecordFromName(SongInfo info, DifficultyEnum difficultyEnum, GameMode gameMode, CancellationToken token, bool deleteNameKeys)
        {
            var currentSongScoreName = SongInfoReader.GetFullSongNameNoID(info, difficultyEnum, gameMode, SCORE);
            var hasScoreRecord = await SongKeyExists(currentSongScoreName, token);
            var currentSongStreakName = SongInfoReader.GetFullSongNameNoID(info, difficultyEnum, gameMode, STREAK);
            var hasStreakRecord = await SongKeyExists(currentSongStreakName, token);

            var exists = hasScoreRecord && hasStreakRecord;

            if (!exists)
            {
                return new SongAndPlaylistRecords();
            }
            var recordScores =
                (SongAndPlaylistScoreRecord[])await GetSongValue<SongAndPlaylistScoreRecord[]>(
                    currentSongScoreName, token);
            var recordStreaks =
                (SongAndPlaylistStreakRecord[])await GetSongValue<SongAndPlaylistStreakRecord[]>(
                    currentSongStreakName, token);

            if (deleteNameKeys)
            {
                DeleteSongKey(currentSongScoreName);
                DeleteSongKey(currentSongStreakName);
            }
            return new SongAndPlaylistRecords(true, recordScores, recordStreaks);

        }
        #endregion
    }
}