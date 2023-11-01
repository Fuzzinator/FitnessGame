using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class NetworkConnectionManager : MonoBehaviour
{
    public static NetworkConnectionManager Instance {get; private set;}

    public UnityEvent<bool> NetworkConnectionUpdated { get; private set; } = new UnityEvent<bool>();
    public bool NetworkConnected { get; private set; } = true;
    private CancellationToken _onDestroyToken;

    private void Awake()
    {
        if (Instance == null)
        { 
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _onDestroyToken = this.GetCancellationTokenOnDestroy();
        MonitorNetworkStatusAsync().Forget();
    }

    private async UniTaskVoid MonitorNetworkStatusAsync()
    {
        while (!_onDestroyToken.IsCancellationRequested)
        {
            var networkConnected = HasNetworkConnection();
            if (NetworkConnected == networkConnected)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(1), cancellationToken: _onDestroyToken);
                continue;
            }

            NetworkConnected = networkConnected;
            NetworkConnectionUpdated?.Invoke(NetworkConnected);
           
            await UniTask.Delay(System.TimeSpan.FromSeconds(1), cancellationToken: _onDestroyToken);
        }
    }

    private static bool HasNetworkConnection()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
}
