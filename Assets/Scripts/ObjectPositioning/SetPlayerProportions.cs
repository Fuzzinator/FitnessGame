using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class SetPlayerProportions : MonoBehaviour, IOrderedInitialize
{
    public bool Initialized { get; private set; }

    [SerializeField]
    private bool _resetOnStart;
    private const string RESETHEADSET = "Reset Headset";


    public void Initialize()
    {
        if (Initialized)
        {
            return;
        }

        if (!_resetOnStart)
        {
            return;
        }
        
        var height = GlobalSettings.TotalHeight;
        if (height < 0)
        {
            ResetHeadsetWithDelay().Forget();
        }
        Initialized = true;
    }

    public void SetHeight()
    {
        GlobalSettings.UserHeight = Head.Instance.transform.position.y;
        GlobalSettings.UserHeightOffset = 0f;
    }

    private async UniTaskVoid ResetHeadsetWithDelay()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: this.GetCancellationTokenOnDestroy(),
            ignoreTimeScale: false);
        
        SetHeight();
    }
}