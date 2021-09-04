using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ScoringManager : MonoBehaviour
{
    public static ScoringManager Instance { get; private set; }

    public int CurrentScore
    {
        get => _currentScore;
        private set
        {
            var oldScore = _currentScore;
            _currentScore = value;
            if (oldScore != _currentScore)
            {
                currentScoreUpdated?.Invoke(_currentScore);
            }
        }
    }

    private int _currentScore;
    
    public UnityEvent<int> currentScoreUpdated = new UnityEvent<int>();

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
        ResetScore();
    }

    public void ResetScore()
    {
        CurrentScore = 0;
    }
    
    public void AddToScore(int valueToAdd)
    {
        CurrentScore += valueToAdd;
    }
}
