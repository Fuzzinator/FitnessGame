using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance { get; private set; }

    private const string TotalWorkoutTime = "TotalWorkoutTime";
    private const string TotalTargetsHit = "TotalTargetsHit";
    private const string TotalObstaclesDodged = "TotalObstaclesDodged";
    private const string RollingLeftHitSpeeds = "RollingLeftHitSpeeds";
    private const string RollingTotalLeftHits = "RollingTotalLeftHits";
    private const string RollingTotalLeftHitSpeed = "RollingTotalLeftHitSpeed";
    private const string RollingRightHitSpeeds = "RollingRightHitSpeeds";
    private const string RollingTotalRightHits = "RollingTotalRightHits";
    private const string RollingTotalRightHitSpeed = "RollingTotalRightHitSpeed";
    private const uint Zero = 0;

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

    public float GetWorkoutTime()
    {
        return SettingsManager.GetSetting(TotalWorkoutTime, 0f);
    }

    public uint GetTargetsHit()
    {
        return SettingsManager.GetSetting(TotalTargetsHit, Zero);
    }

    public uint GetObstaclesDodged()
    {
        return SettingsManager.GetSetting(TotalObstaclesDodged, Zero);
    }
    public void GetLeftHitSpeedStats(ref List<float> hitSpeeds, out int rollingTotalHits, out float rollingTotalHitSpeed)
    {
        hitSpeeds = SettingsManager.GetSetting(RollingLeftHitSpeeds, hitSpeeds);
        rollingTotalHits = SettingsManager.GetSetting(RollingTotalLeftHits, 1);
        rollingTotalHitSpeed = SettingsManager.GetSetting(RollingTotalLeftHitSpeed, SettingsManager.CurrentMinHitSpeed);
    }
    public void GetRightHitSpeedStats(ref List<float> hitSpeeds, out int rollingTotalHits, out float rollingTotalHitSpeed)
    {
        hitSpeeds = SettingsManager.GetSetting(RollingRightHitSpeeds, hitSpeeds);
        rollingTotalHits = SettingsManager.GetSetting(RollingTotalRightHits, 1);
        rollingTotalHitSpeed = SettingsManager.GetSetting(RollingTotalRightHitSpeed, SettingsManager.CurrentMinHitSpeed);
    }

    public void RecordWorkoutTime(float timeInSeconds)
    {
        var previousTime = GetWorkoutTime();
        SettingsManager.SetSetting(TotalWorkoutTime, previousTime + timeInSeconds);
    }

    public void RecordTargetsHit(uint targetsCount)
    {
        var previousTotal = GetTargetsHit();
        SettingsManager.SetSetting(TotalTargetsHit, previousTotal + targetsCount);
    }

    public void RecordObstaclesDodged(uint obstacleCount)
    {
        var previousTotal = GetObstaclesDodged();
        SettingsManager.SetSetting(TotalObstaclesDodged, previousTotal + obstacleCount);
    }

    public void RecordLeftSpeedStats(List<float> hitSpeeds, int rollingTotalHits, float rollingTotalHitSpeed)
    {
        SettingsManager.SetSetting(RollingLeftHitSpeeds, hitSpeeds);
        SettingsManager.SetSetting(RollingTotalLeftHits, rollingTotalHits);
        SettingsManager.SetSetting(RollingTotalLeftHitSpeed, rollingTotalHitSpeed);
    }

    public void RecordRightSpeedStats(List<float> hitSpeeds, int rollingTotalHits, float rollingTotalHitSpeed)
    {
        SettingsManager.SetSetting(RollingRightHitSpeeds, hitSpeeds);
        SettingsManager.SetSetting(RollingTotalRightHits, rollingTotalHits);
        SettingsManager.SetSetting(RollingTotalRightHitSpeed, rollingTotalHitSpeed);
    }
    public async UniTask RecordLeftSpeedStatsAsync(List<float> hitSpeeds, int rollingTotalHits, float rollingTotalHitSpeed)
    {
        SettingsManager.SetSetting(RollingLeftHitSpeeds, hitSpeeds);

        await UniTask.NextFrame();

        SettingsManager.SetSetting(RollingTotalLeftHits, rollingTotalHits);

        await UniTask.NextFrame();

        SettingsManager.SetSetting(RollingTotalLeftHitSpeed, rollingTotalHitSpeed);

        await UniTask.NextFrame();
    }
    public async UniTask RecordRightSpeedStatsAsync(List<float> hitSpeeds, int rollingTotalHits, float rollingTotalHitSpeed)
    {
        SettingsManager.SetSetting(RollingRightHitSpeeds, hitSpeeds);
        
        await UniTask.NextFrame();

        SettingsManager.SetSetting(RollingTotalRightHits, rollingTotalHits);
        
        await UniTask.NextFrame();

        SettingsManager.SetSetting(RollingTotalRightHitSpeed, rollingTotalHitSpeed);
        
        await UniTask.NextFrame();
    }
}
