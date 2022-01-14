using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using InfoSaving;
using UnityEngine;

public class SongAndPlaylistScoreRecorder : MonoBehaviour
{
    private bool _updatingSongRecord;
    private bool _updatingPlaylistRecord;
    private CancellationToken _cancellationToken;

    private void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    public async UniTaskVoid SaveSongStats()
    {
        var songScore = ScoringManager.Instance.ScoreThisSong;
        var bestStreak = StreakManager.Instance.RecordCurrentSongStreak;

        var songFullName = SongInfoReader.Instance.GetSongFullName();

        var oldRecord =
            (SongAndPlaylistRecord) await PlayerStatsFileManager.GetSongValue<SongAndPlaylistRecord>(songFullName,_cancellationToken);

        var newScoreHigher = oldRecord.Score < songScore;
        var newStreakHigher = oldRecord.Streak < bestStreak;

        if (!newScoreHigher && !newStreakHigher)
        {
            return;
        }
        
        if (!newScoreHigher)
        {
            songScore = oldRecord.Score;
        }

        if (!newStreakHigher)
        {
            bestStreak = oldRecord.Streak;
        }
        var newRecord = new SongAndPlaylistRecord(songScore, bestStreak);
        _updatingSongRecord = true;
        await PlayerStatsFileManager.RecordSongValue(songFullName, newRecord, _cancellationToken);
        _updatingPlaylistRecord = false;
    }
}