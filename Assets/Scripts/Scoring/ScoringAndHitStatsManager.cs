using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ScoringAndHitStatsManager : MonoBehaviour, IOrderedInitialize
{
    public static ScoringAndHitStatsManager Instance { get; private set; }
    public bool Initialized { get; private set; }

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

    public int SongScore { get; private set; }
    public uint SongHitTargets { get; private set; }
    public uint SongMissedTargets { get; private set; }
    public uint SongDodgedObstacles { get; private set; }
    public uint SongHitObstacles { get; private set; }

    private float _rollingTotalLeftHitSpeed = -1;
    private float _rollingTotalRightHitSpeed = -1;
    public float RollingTotalLeftHitSpeed
    {
        get
        {
            if (_rollingTotalLeftHitSpeed <= 0)
            {
                _rollingTotalLeftHitSpeed = SettingsManager.CurrentMinHitSpeed;
            }
            return _rollingTotalLeftHitSpeed;
        }
        private set
        {
            _rollingTotalLeftHitSpeed = value;
        }
    }
    public float RollingTotalRightHitSpeed
    {
        get
        {
            if (_rollingTotalRightHitSpeed <= 0)
            {
                _rollingTotalRightHitSpeed = SettingsManager.CurrentMinHitSpeed;
            }
            return _rollingTotalRightHitSpeed;
        }
        private set
        {
            _rollingTotalRightHitSpeed = value;
        }
    }

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

    public void Initialize()
    {
        if (Initialized)
        {
            return;
        }

        FullReset();
        Initialized = true;
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
        ResetSongStatsAsync().Forget();
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
        ResetSongStatsAsync().Forget();
    }

    public async UniTaskVoid ResetSongStatsAsync()
    {
        if(LevelManager.Instance == null || LevelManager.Instance.SkippingSong)
        {
            CurrentScore -= (uint)SongScore;

            SongHitTargets = 0;
            SongDodgedObstacles = 0;

            SongScore = 0;

            SongMissedTargets = 0;
            SongHitObstacles = 0;

            SongPerfectHits = 0;
            SongGreatHits = 0;
            SongGoodHits = 0;
            SongOkayHits = 0;
            SongBadHits = 0;
        }
        if (SongHitTargets > 0)
        {
            StatsManager.Instance.RecordTargetsHit(SongHitTargets);
            await UniTask.NextFrame();
            SongHitTargets = 0;
        }
        if (SongDodgedObstacles > 0)
        {
            StatsManager.Instance.RecordObstaclesDodged(SongDodgedObstacles);
            await UniTask.NextFrame();
            SongDodgedObstacles = 0;
        }

        if (RollingTotalLeftHits > 1)
        {
            await StatsManager.Instance.RecordLeftSpeedStatsAsync(_leftHitSpeeds, RollingTotalLeftHits, RollingTotalLeftHitSpeed);
        }
        else
        {
            StatsManager.Instance.GetLeftHitSpeedStats(ref _leftHitSpeeds, out var totalLeftHits, out var rollingTotalLeftSpeed);
            RollingTotalLeftHits = totalLeftHits;
            RollingTotalLeftHitSpeed = rollingTotalLeftSpeed;
        }

        if (RollingTotalRightHits > 1)
        {
            await StatsManager.Instance.RecordRightSpeedStatsAsync(_rightHitSpeeds, RollingTotalRightHits, RollingTotalRightHitSpeed);
        }
        else
        {
            StatsManager.Instance.GetRightHitSpeedStats(ref _rightHitSpeeds, out var totalRightHits, out var rollingTotalRightSpeed);
            RollingTotalRightHits = totalRightHits;
            RollingTotalRightHitSpeed = rollingTotalRightSpeed;
        }

        SongScore = 0;

        SongMissedTargets = 0;
        SongHitObstacles = 0;

        SongPerfectHits = 0;
        SongGreatHits = 0;
        SongGoodHits = 0;
        SongOkayHits = 0;
        SongBadHits = 0;
    }

    public void AddToScore(float valueToAdd)
    {
        CurrentScore += (uint)(valueToAdd * StreakManager.GetStreakScoreMod());
        SongScore += (int)(valueToAdd * StreakManager.GetCurrentSongScoreMod());
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

            RollingTotalLeftHits = Mathf.Clamp(RollingTotalLeftHits + 1, 1, MaxHistorySize);
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

            RollingTotalRightHits = Mathf.Clamp(RollingTotalRightHits + 1, 1, MaxHistorySize);
        }
        UpdatedHitSpeed.Invoke(hit.HitSpeed);
    }
}
