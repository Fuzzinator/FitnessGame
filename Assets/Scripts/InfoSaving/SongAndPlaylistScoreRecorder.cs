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

    public void SongCompleted()
    {
        SaveSongStats().Forget();
        if (PlaylistManager.Instance.CurrentIndex >= PlaylistManager.Instance.CurrentPlaylist.Items.Length - 1)
        {
            SavePlaylistStats().Forget();
        }
    }

    public async UniTaskVoid SaveSongStats()
    {
        var songFullName = SongInfoReader.Instance.GetSongFullName();
        while (_updatingSongRecord)
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
        }


        ulong songScore;
        int bestStreak;
        var exists = await PlayerStatsFileManager.SongKeyExists(songFullName);
        if (exists)
        {
            var oldRecord =
                (SongAndPlaylistRecord) await PlayerStatsFileManager.GetSongValue<SongAndPlaylistRecord>(songFullName,
                    _cancellationToken);

            if (!ShouldUpdateFile(oldRecord, false, out songScore, out bestStreak))
            {
                return;
            }
        }
        else
        {
            songScore = ScoringManager.Instance.ScoreThisSong;
            bestStreak = StreakManager.Instance.RecordCurrentSongStreak;
        }

        var newRecord = new SongAndPlaylistRecord(songScore, bestStreak);
        _updatingSongRecord = true;
        await PlayerStatsFileManager.RecordSongValue(songFullName, newRecord, _cancellationToken);
        _updatingSongRecord = false;
    }

    public async UniTaskVoid SavePlaylistStats()
    {
        var playlist = PlaylistManager.Instance.CurrentPlaylist;
        while (_updatingPlaylistRecord)
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
        }

        var playlistFullName = $"{playlist.PlaylistName}-{playlist.Length}-{playlist.Items.Length}";

        ulong songScore;
        int bestStreak;
        var exists = await PlayerStatsFileManager.PlaylistKeyExists(playlistFullName);
        if (exists)
        {
            var oldRecord =
                (SongAndPlaylistRecord) await PlayerStatsFileManager.GetPlaylistValue<SongAndPlaylistRecord>(
                    playlistFullName, _cancellationToken);

            if (!ShouldUpdateFile(oldRecord, true, out songScore, out bestStreak))
            {
                return;
            }
        }
        else
        {
            songScore = ScoringManager.Instance.CurrentScore;
            bestStreak = StreakManager.Instance.RecordStreak;
        }

        var newRecord = new SongAndPlaylistRecord(songScore, bestStreak);
        _updatingPlaylistRecord = true;
        await PlayerStatsFileManager.RecordPlaylistValue(playlistFullName, newRecord, _cancellationToken);
        _updatingPlaylistRecord = false;
    }

    private static bool ShouldUpdateFile(SongAndPlaylistRecord oldRecord, bool playlist, out ulong songScore,
        out int bestStreak)
    {
        songScore = playlist ? ScoringManager.Instance.CurrentScore : ScoringManager.Instance.ScoreThisSong;
        bestStreak = playlist ? StreakManager.Instance.RecordStreak : StreakManager.Instance.RecordCurrentSongStreak;

        var newScoreHigher = oldRecord.Score < songScore;
        var newStreakHigher = oldRecord.Streak < bestStreak;
        
        if (!newScoreHigher && !newStreakHigher)
        {
            return false;
        }

        if (!newScoreHigher)
        {
            songScore = oldRecord.Score;
        }

        if (!newStreakHigher)
        {
            bestStreak = oldRecord.Streak;
        }

        return true;
    }
}