using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [SerializeField]
    private Notification _notificationPrefab;

    [Header("Positions")]
    [SerializeField]
    private Transform _basePosition;

    [SerializeField]
    private Transform _popUpPosition;

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
        var headHeight = GlobalSettings.TotalHeight;
        if (headHeight > 0)
        {
            UpdateNotificationHeight(headHeight);
        }

        GlobalSettings.UserHeightChanged.AddListener(UpdateNotificationHeight);
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
        if (visuals.popUp)
        {
            var transform1 = obj.transform;
            transform1.position = Instance._popUpPosition.position;
            transform1.rotation = Instance._popUpPosition.rotation;
        }
        else
        {
            var transform1 = obj.transform;
            transform1.position = Instance._basePosition.position;
            transform1.rotation = Instance._basePosition.rotation;
        }

        return obj;
    }

    public static Notification RequestNotification(Notification.NotificationVisualInfo visuals,
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
        if (visuals.popUp)
        {
            var transform1 = obj.transform;
            transform1.position = Instance._popUpPosition.position;
            transform1.rotation = Instance._popUpPosition.rotation;
        }
        else
        {
            var transform1 = obj.transform;
            transform1.position = Instance._basePosition.position;
            transform1.rotation = Instance._basePosition.rotation;
        }

        return obj;
    }

    public static void ReportFailedToLoadInMenus(string message)
    {
        var visuals = new Notification.NotificationVisuals(message,
            "Failed to load.", autoTimeOutTime: 4, popUp: true);

        RequestNotification(visuals);
    }

    public static void ReportFailedToLoadInGame(string message)
    {
        var visuals = new Notification.NotificationVisuals(message,
            "Failed to load.",
            "Play Next",
            "Main Menu",
            disableUI: false);

        RequestNotification(visuals,
            () => { LevelManager.Instance.LoadNextSong(); },
            () => { ActiveSceneManager.Instance.LoadMainMenu(); }
        );
    }

    private void UpdateNotificationHeight()
    {
        UpdateNotificationHeight(GlobalSettings.TotalHeight);
    }

    private void UpdateNotificationHeight(float newHeight)
    {
        var oldPos = _basePosition.position;
        var newPos = new Vector3(oldPos.x, newHeight, oldPos.z);
        _basePosition.position = newPos;
    }
}