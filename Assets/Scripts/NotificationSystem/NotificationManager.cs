using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [SerializeField]
    private Notification _notificationPrefab;


    private PoolManager _notificationPoolManager;
    
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
        _notificationPoolManager = new PoolManager(_notificationPrefab, transform);
    }

    public static Notification RequestNotification(Notification.NotificationVisuals visuals,
        Action button1Pressed = null, Action button2Pressed = null, Action button3Pressed = null)
    {
        var obj = Instance._notificationPoolManager.GetNewPoolable() as Notification;
        if (obj == null)
        {
            Debug.LogError("Notification was null");
            return null;
        }
        obj.SetUpObject(visuals, button1Pressed, button2Pressed, button3Pressed);
        
        obj.transform.SetParent(Instance.transform);
        return obj;
    }
}
