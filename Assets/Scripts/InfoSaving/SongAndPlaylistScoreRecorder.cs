using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using InfoSaving;
using UnityEngine;
using UnityEngine.Pool;
using static UnityEngine.XR.Hands.XRHandSubsystemDescriptor;

public class SongAndPlaylistScoreRecorder : MonoBehaviour
{
    private bool _updatingSongRecord;
    private bool _updatingPlaylistRecord;
    private CancellationToken _cancellationToken;

    private string _onlineRecordName;
    private string _currentSongRecordName;
    private bool _previousRecordExists = false;
    private readonly SongRecord[] _songRecords = new SongRecord[10];
    private readonly PlaylistRecord[] _playlistRecords = new PlaylistRecord[10];

    private string _profileBestScoreName;
    private SongRecord _profileBestScore;

    private const string STREAK = "Streak:";
    private const string SCORE = "Score:";
    private const string AllowOnlineLeaderboards = "AllowOnlineLeaderboards";
    private const string LocalSongIdentifier = "LOCAL";

    private void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    public void SongStarted()
    {
        if (PlaylistManager.Instance.CurrentIndex < PlaylistManager.Instance.CurrentPlaylist.Items.Length)
        {
            GetPreviousRecord().Forget();
        }
    }

    public void SongCompleted()
    {
        SaveSongStats().Forget();
        if (PlaylistManager.Instance.CurrentIndex >= PlaylistManager.Instance.CurrentPlaylist.Items.Length - 1)
        {
            SavePlaylistStats().Forget();
        }
    }

    private async UniTaskVoid GetPreviousRecord()
    {
        while (_updatingSongRecord || _updatingPlaylistRecord)
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
        }

        var records = await PlaylistManager.Instance.TryGetRecords(_cancellationToken);
        _previousRecordExists = records != null && records.Any((i) => i.IsValid);
        records.CopyTo(_songRecords, 0);

        var currentSongInfo = SongInfoReader.Instance?.songInfo;
        _onlineRecordName = PlaylistManager.Instance.GetOnlineRecordName();
        if (currentSongInfo != null && !string.IsNullOrWhiteSpace(currentSongInfo.SongID))
        {
            _currentSongRecordName = PlaylistManager.Instance.GetFullSongName(noID: false);
        }
        else
        {
            _currentSongRecordName = PlaylistManager.Instance.GetFullSongName(noID: true);
        }
        _profileBestScoreName = $"{ProfileManager.Instance.ActiveProfile.GUID}-HighScore:{_currentSongRecordName}";
        if (PlayerStatsFileManager.SongKeyExists(_profileBestScoreName))
        {
            _profileBestScore = (SongRecord)await PlayerStatsFileManager.GetSongValue<SongRecord>(_profileBestScoreName, _cancellationToken);
        }
        else
        {
            _profileBestScore = new ();
        }
    }

    public async UniTaskVoid SaveSongStats()
    {
        var songScore = ScoringAndHitStatsManager.Instance.SongScore;
        var bestStreak = StreakManager.Instance.RecordCurrentSongStreak;
        TryPostToOnlineLeaderboard();
        while (_updatingSongRecord || _updatingPlaylistRecord)
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
        }

        var scoreIndexToUpdate = -1;

        if (_previousRecordExists)
        {
            for (var i = 0; i < _songRecords.Length; i++)
            {
                var oldRecord = _songRecords[i];
                if (scoreIndexToUpdate < 0 && ShouldUpdateScoreFile(oldRecord, out songScore))
                {
                    scoreIndexToUpdate = i;
                }

                if (scoreIndexToUpdate >= 0)
                {
                    break;
                }
            }
        }
        else
        {

            scoreIndexToUpdate = 0;
        }

        var activeProfile = ProfileManager.Instance.ActiveProfile;


        if (scoreIndexToUpdate >= 0)
        {
            var newScoreRecord = new SongRecord(activeProfile.ProfileName, activeProfile.GUID, (int)songScore, bestStreak);
            InsertInArray(_songRecords, newScoreRecord, scoreIndexToUpdate);
            _updatingSongRecord = true;
            await PlayerStatsFileManager.RecordSongValue(_currentSongRecordName, _songRecords, _cancellationToken);
            _updatingSongRecord = false;
        }
    }

    public async UniTaskVoid SavePlaylistStats()
    {
        var playlist = PlaylistManager.Instance.CurrentPlaylist;
        if (playlist.Items.Length == 1 && string.IsNullOrWhiteSpace(playlist.GUID))
        {
            return;
        }

        while (_updatingSongRecord || _updatingPlaylistRecord)
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
        }

        var playlistFullName = $"{playlist.GUID}-{playlist.DifficultyEnum}-{playlist.TargetGameMode}";
        var recordExists = PlayerStatsFileManager.PlaylistKeyExists(playlistFullName);
        ulong songScore = 0;
        int bestStreak = 0;
        var scoreIndex = -1;
        var updatedFromOld = false;
        if (recordExists)
        {
            try
            {
                var playlistScoreRecords = (PlaylistRecord[])await PlayerStatsFileManager
                    .GetPlaylistValue<PlaylistRecord[]>(
                        playlistFullName, _cancellationToken);

                playlistScoreRecords.CopyTo(_playlistRecords, 0);

                for (var i = 0; i < _playlistRecords.Length; i++)
                {
                    var score = _playlistRecords[i];
                    if (ShouldUpdateScoreFile(score, out songScore))
                    {
                        scoreIndex = i;
                    }

                    if (scoreIndex >= 0)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
        else
        {
            var playlistFullScoreName = $"{SCORE}{playlist.GUID}-{playlist.DifficultyEnum}-{playlist.TargetGameMode}";
            var playlistFullStreakName = $"{STREAK}{playlist.GUID}-{playlist.DifficultyEnum}-{playlist.TargetGameMode}";

            var scoreExists = PlayerStatsFileManager.PlaylistKeyExists(playlistFullScoreName);
            var streakExists = PlayerStatsFileManager.PlaylistKeyExists(playlistFullStreakName);

            if (!scoreExists && !streakExists)
            {
                songScore = ScoringAndHitStatsManager.Instance.CurrentScore;
                bestStreak = StreakManager.Instance.RecordStreak;
                scoreIndex = 0;
            }
            else
            {
                try
                {
                    var playlistScoreRecords = (SongAndPlaylistScoreRecord[])await PlayerStatsFileManager
                        .GetPlaylistValue<SongAndPlaylistScoreRecord[]>(
                            playlistFullScoreName, _cancellationToken);

                    var playlistStreakRecords = (SongAndPlaylistStreakRecord[])await PlayerStatsFileManager
                            .GetPlaylistValue<SongAndPlaylistStreakRecord[]>(
                                playlistFullStreakName, _cancellationToken);

                    PlayerStatsFileManager.DeletePlaylistKey(playlistFullScoreName);
                    PlayerStatsFileManager.DeletePlaylistKey(playlistFullStreakName);
                    ConvertOldRecordsToNew(new SongAndPlaylistRecords(true, playlistScoreRecords, playlistStreakRecords));
                    updatedFromOld = true;
                    for (var i = 0; i < _playlistRecords.Length; i++)
                    {
                        var score = _playlistRecords[i];
                        if (ShouldUpdateScoreFile(score, out songScore))
                        {
                            scoreIndex = i;
                        }

                        if (scoreIndex >= 0)
                        {
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw;
                }
            }
        }

        var activeProfile = ProfileManager.Instance.ActiveProfile;

        if (scoreIndex >= 0 || updatedFromOld)
        {
            var newScoreRecord = new PlaylistRecord(activeProfile.ProfileName, activeProfile.GUID, songScore, bestStreak);
            InsertInArray(_playlistRecords, newScoreRecord, scoreIndex);
            _updatingPlaylistRecord = true;
            await PlayerStatsFileManager.RecordPlaylistValue(playlistFullName, _playlistRecords,
                _cancellationToken);
            _updatingPlaylistRecord = false;
        }
    }

    private void TryPostToOnlineLeaderboard()
    {
        if (!SettingsManager.GetCachedBool(AllowOnlineLeaderboards, false) || string.Equals(_onlineRecordName, LocalSongIdentifier))
        {
            return;
        }
        var songScore = ScoringAndHitStatsManager.Instance.SongScore;
        var songStreak = StreakManager.Instance.RecordCurrentSongStreak;

        if (_profileBestScore.Score >= songScore && _profileBestScore.Streak >= songStreak)
        {
            return;
        }
        var betterScore = _profileBestScore.Score <= songScore ? songScore : _profileBestScore.Score;
        var betterStreak = _profileBestScore.Streak <= songStreak ? songStreak : _profileBestScore.Streak;
        var activeProfile = ProfileManager.Instance.ActiveProfile;
        var recordToRecord = new SongRecord(activeProfile.ProfileName, activeProfile.GUID, betterScore, betterStreak);

        PlayerStatsFileManager.RecordSongValue(_profileBestScoreName, recordToRecord, _cancellationToken).Forget();

        AzureSqlManager.Instance.PostLeaderboardScore(_onlineRecordName, songScore, songStreak, new CancellationToken()).Forget();
    }

    private void ConvertOldRecordsToNew(SongAndPlaylistRecords oldRecord)
    {
        for (int i = 0; i < oldRecord.scores.Length; i++)
        {
            var score = oldRecord.scores[i];
            if (score.IsValid)
            {
                continue;
            }
            string guid = null;
            foreach (var record in _playlistRecords)
            {
                if (!record.IsValid || !string.Equals(record.ProfileName, score.ProfileName))
                {
                    continue;
                }
                guid = record.GUID;
                break;
            }
            int streak = 0;
            foreach (var oldStreak in oldRecord.streaks)
            {
                if (!oldStreak.IsValid || !string.Equals(oldStreak.ProfileName, score.ProfileName))
                {
                    continue;
                }
                var shouldContinue = true;
                foreach (var record in _playlistRecords)
                {
                    if (string.Equals(oldStreak.ProfileName, record.ProfileName) && oldStreak.Streak != record.Streak)
                    {
                        shouldContinue = false;
                        break;
                    }
                }
                if (shouldContinue)
                {
                    continue;
                }
                streak = oldStreak.Streak;
                break;
            }
            _playlistRecords[i] = new PlaylistRecord(score.ProfileName, guid, score.Score, streak);
        }

    }


    private static bool ShouldUpdateScoreFile(SongRecord oldRecord, out int songScore)
    {
        songScore = ScoringAndHitStatsManager.Instance.SongScore;

        var newScoreHigher = oldRecord.Score < songScore;

        return newScoreHigher;
    }
    private static bool ShouldUpdateScoreFile(PlaylistRecord oldRecord, out ulong songScore)
    {
        songScore = ScoringAndHitStatsManager.Instance.CurrentScore;

        var newScoreHigher = oldRecord.Score < songScore;

        return newScoreHigher;
    }

    private static bool ShouldUpdateStreakFile(SongAndPlaylistStreakRecord oldRecord, bool playlist, out int bestStreak)
    {
        bestStreak = playlist ? StreakManager.Instance.RecordStreak : StreakManager.Instance.RecordCurrentSongStreak;

        var newStreakHigher = oldRecord.Streak < bestStreak;

        return newStreakHigher;
    }

    private static void InsertInArray<T>(IList<T> array, T value, int index)
    {
        for (var i = array.Count - 1; i >= index; i--)
        {
            if (i == index)
            {
                array[i] = value;
                break;
            }

            array[i] = array[i - 1];
        }
    }
}