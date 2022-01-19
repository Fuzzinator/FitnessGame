using System;
using UnityEngine;

[Serializable]
public struct SongAndPlaylistRecord
{
    [SerializeField] private ulong _score;
    [SerializeField] private int _streak;
    public ulong Score => _score;
    public int Streak => _streak;

    public SongAndPlaylistRecord(ulong score, int streak)
    {
        _score = score;
        _streak = streak;
    }
}
