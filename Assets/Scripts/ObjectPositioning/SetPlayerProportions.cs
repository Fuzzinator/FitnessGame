using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class SetPlayerProportions : MonoBehaviour
{
    [SerializeField]
    private bool _resetOnStart;
    private const string RESETHEADSET = "Reset Headset";

    // Start is called before the first frame update
    private void Start()
    {
        if (!_resetOnStart)
        {
            return;
        }
        
        var height = GlobalSettings.UserHeight;
        if (height < 0)
        {
            ResetHeadsetWithDelay().Forget();
        }
    }

    public void SetHeight()
    {
        GlobalSettings.UserHeight = Head.Instance.transform.position.y;
    }

    private async UniTaskVoid ResetHeadsetWithDelay()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: this.GetCancellationTokenOnDestroy(),
            ignoreTimeScale: false);
        
        SetHeight();
    }
}