using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using InfoSaving;
using UnityEngine;
using UnityEngine.Pool;

public class SongAndPlaylistScoreRecorder : MonoBehaviour
{
    private bool _updatingSongRecord;
    private bool _updatingPlaylistRecord;
    private CancellationToken _cancellationToken;

    private string _currentSongScoreName;
    private string _currentSongStreakName;
    private bool _previousRecordExists = false;
    private readonly SongAndPlaylistScoreRecord[] _songScoreRecords = new SongAndPlaylistScoreRecord[5];
    private readonly SongAndPlaylistStreakRecord[] _songStreakRecords = new SongAndPlaylistStreakRecord[5];
    private readonly SongAndPlaylistScoreRecord[] _playlistScoreRecords = new SongAndPlaylistScoreRecord[5];
    private readonly SongAndPlaylistStreakRecord[] _playlistStreakRecords = new SongAndPlaylistStreakRecord[5];

    private const string STREAK = "Streak:";
    private const string SCORE = "Score:";

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

        var records =
            await PlaylistManager.Instance.TryGetRecords(_cancellationToken);
        _previousRecordExists = records.hasRecord;
        records.scores.CopyTo(_songScoreRecords, 0);
        records.streaks.CopyTo(_songStreakRecords, 0);

        _currentSongScoreName = PlaylistManager.Instance.GetFullSongName(prefix: SCORE);
        _currentSongStreakName = PlaylistManager.Instance.GetFullSongName(prefix: STREAK);
        /*var hasScoreRecord = await PlayerStatsFileManager.SongKeyExists(_currentSongScoreName);

        var hasStreakRecord = await PlayerStatsFileManager.SongKeyExists(_currentSongStreakName);

        _previousRecordExists = hasScoreRecord && hasStreakRecord;

        if (!_previousRecordExists)
        {
            var oldKey = PlaylistManager.Instance.GetFullSongName();
            _previousRecordExists = await PlayerStatsFileManager.SongKeyExists(oldKey);
            if (_previousRecordExists)
            {
                await UpgradeFromSingleStatsRecord(_songScoreRecords, _songStreakRecords, oldKey);
                PlayerStatsFileManager.DeleteSongKey(oldKey);
            }

            return;
        }

        try
        {
            _songScoreRecords =
                (SongAndPlaylistScoreRecord[]) await PlayerStatsFileManager.GetSongValue<SongAndPlaylistScoreRecord[]>(
                    _currentSongScoreName,
                    _cancellationToken);
            _songStreakRecords =
                (SongAndPlaylistStreakRecord[]) await
                    PlayerStatsFileManager.GetSongValue<SongAndPlaylistStreakRecord[]>(
                        _currentSongScoreName,
                        _cancellationToken);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }*/
    }

    public async UniTaskVoid SaveSongStats()
    {
        while (_updatingSongRecord || _updatingPlaylistRecord)
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
        }

        ulong songScore = 0;
        int bestStreak = 0;
        var scoreIndexToUpdate = -1;
        var streakIndexToUpdate = -1;

        if (_previousRecordExists)
        {
            for (var i = 0; i < _songScoreRecords.Length; i++)
            {
                var oldScoreRecord = _songScoreRecords[i];
                var oldStreakRecord = _songStreakRecords[i];
                if (scoreIndexToUpdate < 0 && ShouldUpdateScoreFile(oldScoreRecord, false, out songScore))
                {
                    scoreIndexToUpdate = i;
                }

                if (streakIndexToUpdate < 0 && ShouldUpdateStreakFile(oldStreakRecord, false, out bestStreak))
                {
                    streakIndexToUpdate = i;
                }

                if (scoreIndexToUpdate >= 0 && streakIndexToUpdate >= 0)
                {
                    break;
                }
            }
        }
        else
        {
            songScore = ScoringManager.Instance.ScoreThisSong;
            bestStreak = StreakManager.Instance.RecordCurrentSongStreak;

            scoreIndexToUpdate = 0;
            streakIndexToUpdate = 0;
        }

        var activeProfile = ProfileManager.Instance.ActiveProfile;


        if (scoreIndexToUpdate >= 0)
        {
            var newScoreRecord =
                new SongAndPlaylistScoreRecord(songScore, activeProfile.ProfileName, activeProfile.GUID);
            InsertInArray(_songScoreRecords, newScoreRecord, scoreIndexToUpdate);
            _updatingSongRecord = true;
            await PlayerStatsFileManager.RecordSongValue(_currentSongScoreName, _songScoreRecords, _cancellationToken);
            _updatingSongRecord = false;
        }


        if (streakIndexToUpdate >= 0)
        {
            var newStreakRecord =
                new SongAndPlaylistStreakRecord(bestStreak, activeProfile.ProfileName, activeProfile.GUID);
            InsertInArray(_songStreakRecords, newStreakRecord, streakIndexToUpdate);
            _updatingSongRecord = true;
            await PlayerStatsFileManager.RecordSongValue(_currentSongStreakName, _songStreakRecords,
                _cancellationToken);
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

        var playlistFullScoreName = $"{SCORE}{playlist.GUID}-{playlist.DifficultyEnum}-{playlist.TargetGameMode}";
        var playlistFullStreakName = $"{STREAK}{playlist.GUID}-{playlist.DifficultyEnum}-{playlist.TargetGameMode}";

        ulong songScore = 0;
        var bestStreak = 0;
        var scoreExists = await PlayerStatsFileManager.PlaylistKeyExists(playlistFullScoreName);
        var streakExists = await PlayerStatsFileManager.PlaylistKeyExists(playlistFullStreakName);
        var scoreIndex = -1;
        var streakIndex = -1;

        if (!scoreExists && !streakExists)
        {
            var oldKey = $"{playlist.PlaylistName}-{playlist.Length}-{playlist.Items.Length}";
            var keyExists = await PlayerStatsFileManager.SongKeyExists(oldKey);
            if (keyExists)
            {
                #region Upgrading
                var upgraded = await PlayerStatsFileManager.UpgradeFromSingleStatsRecord(oldKey, _cancellationToken);
                PlayerStatsFileManager.DeletePlaylistKey(oldKey);
                if (ShouldUpdateScoreFile(upgraded.scores[0], true, out songScore))
                {
                    scoreIndex = 0;
                }

                if (ShouldUpdateStreakFile(upgraded.streaks[0], true, out bestStreak))
                {
                    streakIndex = 0;
                }

                #endregion
            }
            else
            {
                songScore = ScoringManager.Instance.CurrentScore;
                bestStreak = StreakManager.Instance.RecordStreak;
                scoreIndex = 0;
                streakIndex = 0;
            }
        }
        else
        {
            try
            {
                var playlistScoreRecords = (SongAndPlaylistScoreRecord[])await PlayerStatsFileManager
                    .GetPlaylistValue<SongAndPlaylistScoreRecord[]>(
                        playlistFullScoreName, _cancellationToken);

                playlistScoreRecords.CopyTo(_playlistScoreRecords, 0);

                var playlistStreakRecords = (SongAndPlaylistStreakRecord[])await PlayerStatsFileManager
                        .GetPlaylistValue<SongAndPlaylistStreakRecord[]>(
                            playlistFullStreakName, _cancellationToken);

                playlistStreakRecords.CopyTo(_playlistStreakRecords, 0);

                for (var i = 0; i < _playlistScoreRecords.Length; i++)
                {
                    var score = _playlistScoreRecords[i];
                    var streak = _playlistStreakRecords[i];
                    if (ShouldUpdateScoreFile(score, true, out songScore))
                    {
                        scoreIndex = i;
                    }

                    if (ShouldUpdateStreakFile(streak, true, out bestStreak))
                    {
                        streakIndex = i;
                    }

                    if (scoreIndex >= 0 && streakIndex >= 0)
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

        var activeProfile = ProfileManager.Instance.ActiveProfile;

        if (scoreIndex >= 0)
        {
            var newScoreRecord =
                new SongAndPlaylistScoreRecord(songScore, activeProfile.ProfileName, activeProfile.GUID);
            InsertInArray(_playlistScoreRecords, newScoreRecord, scoreIndex);
            _updatingPlaylistRecord = true;
            await PlayerStatsFileManager.RecordPlaylistValue(playlistFullScoreName, _playlistScoreRecords,
                _cancellationToken);
            _updatingPlaylistRecord = false;
        }


        if (streakIndex >= 0)
        {
            var newStreakRecord =
                new SongAndPlaylistStreakRecord(bestStreak, activeProfile.ProfileName, activeProfile.GUID);
            InsertInArray(_playlistStreakRecords, newStreakRecord, streakIndex);
            _updatingPlaylistRecord = true;
            await PlayerStatsFileManager.RecordPlaylistValue(playlistFullStreakName, _playlistStreakRecords,
                _cancellationToken);
            _updatingPlaylistRecord = false;
        }
    }

    private static bool ShouldUpdateScoreFile(SongAndPlaylistScoreRecord oldRecord, bool playlist, out ulong songScore)
    {
        songScore = playlist ? ScoringManager.Instance.CurrentScore : ScoringManager.Instance.ScoreThisSong;

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