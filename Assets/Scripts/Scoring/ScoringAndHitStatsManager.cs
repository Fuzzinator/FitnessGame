using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ScoringAndHitStatsManager : MonoBehaviour
{
    public static ScoringAndHitStatsManager Instance { get; private set; }

    public uint CurrentScore
    {
        get => _currentScore;
        private set
        {
            var oldScore = _currentScore;
            _currentScore = value;
            if (oldScore != _currentScore)
            {
                currentScoreUpdated?.Invoke((_currentScore - oldScore));
            }
        }
    }
    public uint SongPerfectHits { get; private set; }
    public uint SongGreatHits { get; private set; }
    public uint SongGoodHits { get; private set; }
    public uint SongOkayHits { get; private set; }
    public uint SongBadHits { get; private set; }
    public uint SongMissedHits { get; private set; }
    public uint WorkoutPerfectHits { get; private set; }
    public uint WorkoutGreatHits { get; private set; }
    public uint WorkoutGoodHits { get; private set; }
    public uint WorkoutOkayHits { get; private set; }
    public uint WorkoutBadHits { get; private set; }
    public uint WorkoutMissedHits { get; private set; }

    public uint WorkoutHitTargets { get; private set; }
    public uint WorkoutMissedTargets { get; private set; }
    public uint WorkoutDodgedObstacles { get; private set; }
    public uint WorkoutHitObstacles { get; private set; }

    public ulong SongScore { get; private set; }
    public uint SongHitTargets { get; private set; }
    public uint SongMissedTargets { get; private set; }
    public uint SongDodgedObstacles { get; private set; }
    public uint SongHitObstacles { get; private set; }

    public float RollingTotalLeftHitSpeed { get; private set; } = SettingsManager.DefaultMinHitSpeed;
    public float RollingTotalRightHitSpeed { get; private set; } = SettingsManager.DefaultMinHitSpeed;
    public int RollingTotalLeftHits { get; private set; } = 1;
    public int RollingTotalRightHits { get; private set; } = 1;

    public float AverageLeftHitSpeed => RollingTotalLeftHitSpeed / RollingTotalLeftHits;
    public float AverageRightHitSpeed => RollingTotalRightHitSpeed / RollingTotalRightHits;
    public float AverageTotalHitSpeed => (AverageLeftHitSpeed + AverageRightHitSpeed) * .5f;

    private List<float> _leftHitSpeeds = new List<float>(MaxHistorySize + 1);
    private List<float> _rightHitSpeeds = new List<float>(MaxHistorySize + 1);

    [SerializeField]
    private uint _currentScore;

    public UnityEvent<uint> currentScoreUpdated = new UnityEvent<uint>();
    public UnityEvent<float> UpdatedHitSpeed { get; private set; } = new UnityEvent<float>();

    private const int MaxHistorySize = 100;

    private bool _recordedOnEndScene = false;

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
        _recordedOnEndScene = false;
    }

    private void Start()
    {
        FullReset();
        GetAverageSpeedStats();
    }

    private void OnDisable()
    {
        RecordStats();
        _recordedOnEndScene = true;
    }

    private void OnDestroy()
    {
        if (_recordedOnEndScene)
        {
            return;
        }
        RecordStats();
    }

    private void RecordStats()
    {
        StatsManager.Instance.RecordTargetsHit(SongHitTargets);
        StatsManager.Instance.RecordObstaclesDodged(SongDodgedObstacles);
        StatsManager.Instance.RecordLeftSpeedStats(_leftHitSpeeds, RollingTotalLeftHits, RollingTotalLeftHitSpeed);
        StatsManager.Instance.RecordRightSpeedStats(_rightHitSpeeds, RollingTotalRightHits, RollingTotalRightHitSpeed);
    }

    public void FullReset()
    {
        ResetWorkoutStats();
        ResetSongStats();
    }

    private void ResetWorkoutStats()
    {
        CurrentScore = 0;

        WorkoutHitTargets = 0;
        WorkoutMissedTargets = 0;
        WorkoutDodgedObstacles = 0;
        WorkoutHitObstacles = 0;

        WorkoutPerfectHits = 0;
        WorkoutGreatHits = 0;
        WorkoutGoodHits = 0;
        WorkoutOkayHits = 0;
        WorkoutBadHits = 0;
    }
    public void ResetSongStats()
    {
        StatsManager.Instance.RecordTargetsHit(SongHitTargets);
        StatsManager.Instance.RecordObstaclesDodged(SongDodgedObstacles);
        StatsManager.Instance.RecordLeftSpeedStats(_leftHitSpeeds, RollingTotalLeftHits, RollingTotalLeftHitSpeed);
        StatsManager.Instance.RecordRightSpeedStats(_rightHitSpeeds, RollingTotalRightHits, RollingTotalRightHitSpeed);
        SongScore = 0;

        SongHitTargets = 0;
        SongMissedTargets = 0;
        SongDodgedObstacles = 0;
        SongHitObstacles = 0;

        SongPerfectHits = 0;
        SongGreatHits = 0;
        SongGoodHits = 0;
        SongOkayHits = 0;
        SongBadHits = 0;
    }

    private void GetAverageSpeedStats()
    {
        StatsManager.Instance.GetLeftHitSpeedStats(ref _leftHitSpeeds, out var totalLeftHits, out var rollingTotalLeftSpeed);
        RollingTotalLeftHits = totalLeftHits;
        RollingTotalLeftHitSpeed = rollingTotalLeftSpeed;
        StatsManager.Instance.GetRightHitSpeedStats(ref _rightHitSpeeds, out var totalRightHits, out var rollingTotalRightSpeed);
        RollingTotalRightHits = totalRightHits;
        RollingTotalRightHitSpeed = rollingTotalRightSpeed;
    }

    public void AddToScore(float valueToAdd)
    {
        CurrentScore += (uint)(valueToAdd * StreakManager.GetStreakScoreMod());
        SongScore += (uint)(valueToAdd * StreakManager.GetCurrentSongScoreMod());
    }

    public void RegisterHitTarget(HitInfo hit)
    {
        switch (hit.QualityName)
        {
            case HitInfo.HitQualityName.Bad:
                SongBadHits++;
                WorkoutBadHits++;
                break;
            case HitInfo.HitQualityName.Okay:
                SongOkayHits++;
                WorkoutOkayHits++;
                break;
            case HitInfo.HitQualityName.Good:
                SongGoodHits++;
                WorkoutGoodHits++;
                break;
            case HitInfo.HitQualityName.Great:
                SongGreatHits++;
                WorkoutGreatHits++;
                break;
            case HitInfo.HitQualityName.Perfect:
                SongPerfectHits++;
                WorkoutPerfectHits++;
                break;
        }
        SongHitTargets++;
        WorkoutHitTargets++;
    }

    public void RegisterMissedTarget()
    {
        SongMissedTargets++;
        WorkoutMissedTargets++;
    }

    public void RegisterDodgedObstacle()
    {
        SongDodgedObstacles++;
        WorkoutDodgedObstacles++;
    }

    public void RegisterHitObstacle()
    {
        SongHitObstacles++;
        WorkoutHitObstacles++;
    }

    public void RecordHitSpeed(HitInfo hit)
    {
        if (hit.HitHand.AssignedHand == HitSideType.Left)
        {
            RollingTotalLeftHitSpeed += hit.HitSpeed;
            _leftHitSpeeds.Insert(0, hit.HitSpeed);
            if (_leftHitSpeeds.Count > MaxHistorySize)
            {
                RollingTotalLeftHitSpeed -= _leftHitSpeeds[MaxHistorySize];
                _leftHitSpeeds.RemoveAt(MaxHistorySize);
            }
            if (RollingTotalLeftHits < MaxHistorySize)
            {
                RollingTotalLeftHits++;
            }
            else
            {
                RollingTotalLeftHits = MaxHistorySize;
            }
        }
        else if (hit.HitHand.AssignedHand == HitSideType.Right)
        {
            RollingTotalRightHitSpeed += hit.HitSpeed;
            _rightHitSpeeds.Insert(0, hit.HitSpeed);
            if (_rightHitSpeeds.Count > MaxHistorySize)
            {
                RollingTotalRightHitSpeed -= _rightHitSpeeds[MaxHistorySize];
                _rightHitSpeeds.RemoveAt(MaxHistorySize);
            }
            if (RollingTotalRightHits < MaxHistorySize)
            {
                RollingTotalRightHits++;
            }
            else
            {
                RollingTotalRightHits = MaxHistorySize;
            }
        }
        UpdatedHitSpeed.Invoke(hit.HitSpeed);
    }
}
