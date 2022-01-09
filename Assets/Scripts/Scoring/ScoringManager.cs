using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ScoringManager : MonoBehaviour
{
    public static ScoringManager Instance { get; private set; }

    public ulong CurrentScore
    {
        get => _currentScore;
        private set
        {
            var oldScore = _currentScore;
            _currentScore = value;
            if (oldScore != _currentScore)
            {
                currentScoreUpdated?.Invoke((uint)(_currentScore-oldScore));
            }
        }
    }

    public uint GoodHits => _goodHits;
    public uint MissedTargets => _missedTargets;
    public uint HitObstacles => _hitObstacles;

    public ulong ScoreThisSong => _scoreThisSong;
    public uint GoodHitsThisSong => _goodHitsThisSong;
    public uint MissedTargetsThisSong  => _missedTargetsThisSong;
    public uint HitObstaclesThisSong  => _hitObstaclesThisSong;
    
    [SerializeField]
    private ulong _currentScore;

    private uint _goodHits;
    private uint _missedTargets;
    private uint _hitObstacles;

    private ulong _scoreThisSong;
    private uint _goodHitsThisSong;
    private uint _missedTargetsThisSong;
    private uint _hitObstaclesThisSong;
    
    public UnityEvent<uint> currentScoreUpdated = new UnityEvent<uint>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        FullReset();
    }

    public void FullReset()
    {
        ResetScore();
        ResetHitsAndMisses();
        NewSongStarted();
    }
    
    private void ResetScore()
    {
        CurrentScore = 0;
    }

    private void ResetHitsAndMisses()
    {
        _goodHits = 0;
        _missedTargets = 0;
        _hitObstacles = 0;
    }
    
    public void AddToScore(float valueToAdd)
    {
        CurrentScore += (ulong)(valueToAdd*StreakManager.GetStreakScoreMod());
        _scoreThisSong += (ulong)(valueToAdd*StreakManager.GetCurrentSongScoreMod());
    }

    public void RegisterHit()
    {
        _goodHits++;
        _goodHitsThisSong++;
    }

    public void RegisterMiss(bool wasTarget = true)
    {
        if (wasTarget)
        {
            _missedTargets++;
            _missedTargetsThisSong++;
        }
        else
        {
            _hitObstacles++;
            _hitObstaclesThisSong++;
        }
    }

    public void NewSongStarted()
    {
        _scoreThisSong = 0;
        _goodHitsThisSong = 0;
        _missedTargetsThisSong = 0;
        _hitObstaclesThisSong = 0;
    }
}
