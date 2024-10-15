using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOrderedInitialize
{
    public bool Initialized { get; }
    public void Initialize();
}

[System.Serializable]
public class IOrderedInitializeContainer : IUnifiedContainer<IOrderedInitialize> 
{
    public bool Initialized => Result.Initialized;
    public void Initialize() => Result.Initialize();
}
