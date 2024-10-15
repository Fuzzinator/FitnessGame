using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IManagedOnAwake
{
   public void OnAwake();
}


[System.Serializable]
public class IManagedOnEnableContainer : IUnifiedContainer<IManagedOnAwake>
{
    public void OnAwake() => Result.OnAwake();
}