using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StreakManager : MonoBehaviour
{
    public static StreakManager Instance { get; private set; }

    [SerializeField]
    private int _currentStreak = 0;

    [SerializeField]
    private int _recordStreak = 0;

    private int _recordModifier = 1;

    [SerializeField]
    private UnityEvent<int> _currentStreakChanged = new UnityEvent<int>();

    [SerializeField]
    private UnityEvent<int> _recordStreakChanged = new UnityEvent<int>();
    
    [SerializeField]
    private UnityEvent<int> _streakModifierChanged = new UnityEvent<int>();

    private const int MULTIPLIERBASE = 2;

    public int CurrentStreak
    {
        get => _currentStreak;
        private set
        {
            if (_currentStreak != value)
            {
                _currentStreak = value;
                if (_currentStreak > _recordStreak)
                {
                    RecordStreak = _currentStreak;
                }
                _currentStreakChanged?.Invoke(value);
            }
        }
    }
    public int RecordStreak
    {
        get => _recordStreak;
        private set
        {
            if (_recordStreak != value)
            {
                _recordStreak = value;
                _recordStreakChanged?.Invoke(value);
            }
        }
    }

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

    public void IncreaseStreak(int increase = 1)
    {
        CurrentStreak += increase;
    }

    public void ResetStreak()
    {
        CurrentStreak = 0;
    }

    public static int GetStreakScoreMod()
    {
        var multiplier = 1;
        var powerOf = 1;
        
        while (Instance.CurrentStreak >= MULTIPLIERBASE * powerOf)
        {
            powerOf *= MULTIPLIERBASE;
            multiplier++;
        }

        if (multiplier != Instance._recordModifier)
        {
            Instance._recordModifier = multiplier;
            Instance._streakModifierChanged?.Invoke(multiplier);
        }
        
        return Instance._recordModifier;
    }
}