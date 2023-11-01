using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class NetworkConnectionValidator : MonoBehaviour, INetworkConnectivityListener
{
    [SerializeField]
    private UnityEvent _networkConnectionLost;

    [SerializeField]
    private UnityEvent _networkConnectionGained;

    private Notification.NotificationVisuals _lostConnection;
    private Notification.NotificationVisuals _gainedConnection;
    private INetworkConnectivityListener _listener;

    private void Start()
    {
        _lostConnection = new Notification.NotificationVisuals("Online content, including community, content will be disabled until an internet connection can be established.",
                    "No Internet Connection.", disableUI: true, popUp: true, autoTimeOutTime:2f);
        _gainedConnection = new Notification.NotificationVisuals("Online content, including community, is now available.",
                    "Acquired Internet Connection.", disableUI: true, popUp: true, autoTimeOutTime: 2f);
        _listener = this;
        _listener.AddListener();
    }

    private void OnDestroy()
    {
        _listener.RemoveListener();
    }

    void INetworkConnectivityListener.NetworkConnectionListener(bool connected)
    {
        if (connected)
        {
            NotificationManager.RequestNotification(_gainedConnection);
            _networkConnectionGained?.Invoke();
        }
        else
        {
            NotificationManager.RequestNotification(_lostConnection);
            _networkConnectionLost?.Invoke();
        }
    }
}
