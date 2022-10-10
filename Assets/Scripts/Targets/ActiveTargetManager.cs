using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActiveTargetManager : MonoBehaviour
{
    public static ActiveTargetManager Instance { get; private set; }
    
    public List<BaseTarget> activeTargets = new List<BaseTarget>();
    public UnityEvent<BaseTarget> newActiveTarget = new UnityEvent<BaseTarget>();
    public UnityEvent<BaseTarget> targetDeactivated = new UnityEvent<BaseTarget>();

    public List<BaseObstacle> activeObstacles = new List<BaseObstacle>();
    public UnityEvent<BaseObstacle> newActiveObstacle = new UnityEvent<BaseObstacle>();
    public UnityEvent<BaseObstacle> obstacleDeactivated = new UnityEvent<BaseObstacle>();
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

    public void AddActiveTarget(BaseTarget target)
    {
        activeTargets.Add(target);
        newActiveTarget?.Invoke(target);
    }

    public void RemoveActiveTarget(BaseTarget target)
    {
        activeTargets.Remove(target);
        targetDeactivated?.Invoke(target);
    }

    public void AddActiveObstacle(BaseObstacle obstacle)
    {
        activeObstacles.Add(obstacle);
        newActiveObstacle?.Invoke(obstacle);
    }

    public void RemoveActiveObstacle(BaseObstacle obstacle)
    {
        activeObstacles.Remove(obstacle);
        obstacleDeactivated?.Invoke(obstacle);
    }
}
