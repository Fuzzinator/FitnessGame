using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActiveTargetManager : MonoBehaviour
{
    public static ActiveTargetManager Instance { get; private set; }
    
    public List<BaseTarget> activeTargets;
    public UnityEvent<BaseTarget> newActiveTarget = new UnityEvent<BaseTarget>();
    public UnityEvent<BaseTarget> targetDeactivated = new UnityEvent<BaseTarget>();
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
}
