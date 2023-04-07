using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class NetworkConnectionValidator : MonoBehaviour
{
    [SerializeField]
    private UnityEvent _networkConnectionLost;

    [SerializeField]
    private UnityEvent _networkConnectionGained;

    //private NetworkReachability _currentNetworkStatus = NetworkReachability.ReachableViaLocalAreaNetwork;
    private bool _networkConnected = true;
    private CancellationToken _onDestroyToken;

    private Notification.NotificationVisuals _lostConnection;
    private Notification.NotificationVisuals _gainedConnection;

    // Start is called before the first frame update
    void Start()
    {
        _onDestroyToken = CancellationToken.None;
        _lostConnection = new Notification.NotificationVisuals("Online content, including community, content will be disabled until an internet connection can be established.",
                    "No Internet Connection.", disableUI: true, popUp: true, autoTimeOutTime:2f);
        _gainedConnection = new Notification.NotificationVisuals("Online content, including community, is now available.",
                    "Acquired Internet Connection.", disableUI: true, popUp: true, autoTimeOutTime: 2f);
        MonitorNetworkStatusAsync().Forget();
    }

    private async UniTaskVoid MonitorNetworkStatusAsync()
    {
        while (!_onDestroyToken.IsCancellationRequested)
        {
            var networkConnected = HasNetworkConnection();
            if (_networkConnected == networkConnected)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(1), cancellationToken: _onDestroyToken);
                continue;
            }

            _networkConnected = networkConnected;
            if (_networkConnected)
            {
                NotificationManager.RequestNotification(_gainedConnection);
                _networkConnectionGained?.Invoke();
            }
            else
            {
                NotificationManager.RequestNotification(_lostConnection);
                _networkConnectionLost?.Invoke();
            }
            await UniTask.Delay(System.TimeSpan.FromSeconds(1), cancellationToken: _onDestroyToken);
        }
    }

    public static bool HasNetworkConnection()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
}
