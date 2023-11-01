using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INetworkConnectivityListener
{
    public virtual void AddListener()
    {
        NetworkConnectionManager.Instance.NetworkConnectionUpdated.AddListener(NetworkConnectionListener);
    }

    public void RemoveListener()
    {
        NetworkConnectionManager.Instance.NetworkConnectionUpdated.RemoveListener(NetworkConnectionListener);
    }

    protected abstract void NetworkConnectionListener(bool connected);
}
