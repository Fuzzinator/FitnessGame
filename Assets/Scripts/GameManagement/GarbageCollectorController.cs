using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

public class GarbageCollectorController : MonoBehaviour
{
    private CancellationToken _cancellationToken;
    
    // Start is called before the first frame update
    void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
        SetGarbageCollectorState(false);
        //BackgroundGarbageCollectorAsync().Forget();
    }

    private void OnDestroy()
    {
        SetGarbageCollectorState(true);
    }

    public void SetGarbageCollectorState(bool enabled)
    {
        #if !UNITY_EDITOR
        GarbageCollector.GCMode = enabled ? 
            GarbageCollector.Mode.Enabled :
            GarbageCollector.Mode.Manual;
        #endif
    }

    public void RunGarbageCollector()
    {
        RunGarbageCollectorAsync(1000000).Forget();
    }

    private async UniTask RunGarbageCollectorAsync(ulong nanoseconds)
    {
        while(GarbageCollector.CollectIncremental(nanoseconds))
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
            if (_cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private async UniTaskVoid BackgroundGarbageCollectorAsync()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            //GarbageCollector.CollectIncremental(10000);
            await RunGarbageCollectorAsync(10000);
            await UniTask.Delay(TimeSpan.FromSeconds(5f), cancellationToken:_cancellationToken);
        }
    }
}