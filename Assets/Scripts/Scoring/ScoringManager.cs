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

    [SerializeField]
    private ulong _currentScore;

    private uint _goodHits;
    private uint _missedTargets;
    private uint _hitObstacles;
    
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
    
    public void AddToScore(int valueToAdd)
    {
        CurrentScore += (ulong)valueToAdd;
    }

    public void RegisterHit()
    {
        _goodHits++;
    }

    public void RegisterMiss(bool wasTarget = true)
    {
        if (wasTarget)
        {
            _missedTargets++;
        }
        else
        {
            _hitObstacles++;
        }
    }
}
