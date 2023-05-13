using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance { get; private set; }

    private const string TotalWorkoutTime = "TotalWorkoutTime";
    private const string TotalTargetsHit = "TotalTargetsHit";
    private const string TotalObstaclesDodged = "TotalObstaclesDodged";

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

    public int GetTargetsHit()
    {
        return SettingsManager.GetSetting(TotalTargetsHit, 0);
    }

    public int GetObstaclesDodged()
    {
        return SettingsManager.GetSetting(TotalObstaclesDodged, 0);
    }
    
    public void RecordWorkoutTime(float timeInSeconds)
    {
        var previousTime = GetWorkoutTime();
        SettingsManager.SetSetting(TotalWorkoutTime, previousTime+timeInSeconds);
    }

    public void RecordTargetsHit(int targetsCount)
    {
        var previousTotal = GetTargetsHit();
        SettingsManager.SetSetting(TotalTargetsHit, previousTotal + targetsCount);
    }
    
    public void RecordObstaclesDodged(int obstacleCount)
    {
        var previousTotal = GetObstaclesDodged();
        SettingsManager.SetSetting(TotalObstaclesDodged, previousTotal+obstacleCount);
    }
}
