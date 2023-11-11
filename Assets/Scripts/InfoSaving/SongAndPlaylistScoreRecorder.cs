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
using static UnityEngine.XR.Hands.XRHandSubsystemDescriptor;

public class SongAndPlaylistScoreRecorder : MonoBehaviour
{
    private bool _updatingSongRecord;
    private bool _updatingPlaylistRecord;
    private CancellationToken _cancellationToken;

    private string _currentSongRecordName;
    private bool _previousRecordExists = false;
    private readonly SongRecord[] _songRecords = new SongRecord[10];
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

        var records = await PlaylistManager.Instance.TryGetRecords(_cancellationToken);
        records.CopyTo(_songRecords, 0);

        var currentSongInfo = SongInfoReader.Instance?.songInfo;
        if (currentSongInfo != null && !string.IsNullOrWhiteSpace(currentSongInfo.SongID))
        {
            _currentSongRecordName = PlaylistManager.Instance.GetFullSongName(noID: false);
        }
        else
        {
            _currentSongRecordName = PlaylistManager.Instance.GetFullSongName(noID: true);
        }

    }

    public async UniTaskVoid SaveSongStats()
    {
        while (_updatingSongRecord || _updatingPlaylistRecord)
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
        }

        int songScore = 0;
        int bestStreak = 0;
        var scoreIndexToUpdate = -1;

        if (_previousRecordExists)
        {
            for (var i = 0; i < _songRecords.Length; i++)
            {
                var oldRecord = _songRecords[i];
                if (scoreIndexToUpdate < 0 && ShouldUpdateScoreFile(oldRecord, false, out var scoreAsLong))
                {
                    songScore = (int)scoreAsLong;
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
            songScore = ScoringAndHitStatsManager.Instance.SongScore;
            bestStreak = StreakManager.Instance.RecordCurrentSongStreak;

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

        var playlistFullScoreName = $"{SCORE}{playlist.GUID}-{playlist.DifficultyEnum}-{playlist.TargetGameMode}";
        var playlistFullStreakName = $"{STREAK}{playlist.GUID}-{playlist.DifficultyEnum}-{playlist.TargetGameMode}";

        ulong songScore = 0;
        var bestStreak = 0;
        var scoreExists = PlayerStatsFileManager.PlaylistKeyExists(playlistFullScoreName);
        var streakExists = PlayerStatsFileManager.PlaylistKeyExists(playlistFullStreakName);
        var scoreIndex = -1;
        var streakIndex = -1;

        if (!scoreExists && !streakExists)
        {
            songScore = ScoringAndHitStatsManager.Instance.CurrentScore;
            bestStreak = StreakManager.Instance.RecordStreak;
            scoreIndex = 0;
            streakIndex = 0;
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

    private static bool ShouldUpdateScoreFile(SongRecord oldRecord, bool playlist, out ulong songScore)
    {
        songScore = playlist ? ScoringAndHitStatsManager.Instance.CurrentScore : (uint)ScoringAndHitStatsManager.Instance.SongScore;

        var newScoreHigher = (ulong)oldRecord.Score < songScore;

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