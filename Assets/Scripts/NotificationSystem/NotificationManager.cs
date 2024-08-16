using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Notification;

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

    private static Queue<NotificationRequest> _notificationsQueue = new Queue<NotificationRequest>();
    private static Notification _activeNotification;

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

    public static void RequestNotification(NotificationVisuals visuals,
        Action button1Pressed = null, Action button2Pressed = null, Action button3Pressed = null)
    {
        var id = Guid.NewGuid();
        _notificationsQueue.Enqueue(new NotificationRequest(visuals, button1Pressed, button2Pressed, button3Pressed, id));
        if (_activeNotification == null)
        {
            SpawnNextNotification();
        }
    }

    public static async UniTask<Notification> RequestNotificationAsync(NotificationVisuals visuals,
        Action button1Pressed = null, Action button2Pressed = null, Action button3Pressed = null)
    {
        var id = Guid.NewGuid();
        _notificationsQueue.Enqueue(new NotificationRequest(visuals, button1Pressed, button2Pressed, button3Pressed, id));
        if (_activeNotification == null)
        {
            SpawnNextNotification();
        }

        await UniTask.WaitWhile(() => _activeNotification.RequestID != id);

        return _activeNotification;
    }

    private static void SpawnNextNotification()
    {
        if (_notificationsQueue.Count == 0)
        {
            return;
        }
        var notification = _notificationsQueue.Dequeue();

        var obj = Instance._notificationPoolManager.GetNewPoolable() as Notification;
        if (obj == null)
        {
            Debug.LogError("Notification was null");
            SpawnNextNotification();
            return;
        }

        obj.SetUpObject(notification.NotificationVisuals, notification.RequestID, notification.Button1, notification.Button2, notification.Button3);

        obj.transform.SetParent(Instance.transform);
        if (notification.NotificationVisuals.popUp)
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

        _activeNotification = obj;
    }

    public static void RequestNotification(NotificationVisualInfo visuals,
        Action button1Pressed = null, Action button2Pressed = null, Action button3Pressed = null)
    {
        RequestNotification(new NotificationVisuals(visuals), button1Pressed, button2Pressed, button3Pressed);
    }

    public static void ReportFailedToLoadInMenus(string message)
    {
        var visuals = new NotificationVisuals(message,
            "Failed to load.", autoTimeOutTime: 4, popUp: true);

        RequestNotification(visuals);
    }

    public static void ReportFailedToLoadInGame(string message)
    {
        var visuals = new NotificationVisuals(message,
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

    public bool TryGetNextNotification(Guid notificationID)
    {
        _activeNotification = null;
        if (_activeNotification == null || _activeNotification.RequestID != notificationID || _notificationsQueue.Count == 0)
        {
            return false;
        }

        SpawnNextNotification();

        return true;
    }

    private struct NotificationRequest
    {
        public readonly NotificationVisuals NotificationVisuals { get; }
        public readonly Action Button1 { get; }
        public readonly Action Button2 { get; }
        public readonly Action Button3 { get; }
        public readonly Guid RequestID { get; }

        public NotificationRequest(NotificationVisuals visuals,
        Action button1Pressed, Action button2Pressed, Action button3Pressed, Guid requestID)
        {
            NotificationVisuals = visuals;
            Button1 = button1Pressed;
            Button2 = button2Pressed;
            Button3 = button3Pressed;
            RequestID = requestID;
        }
    }
}