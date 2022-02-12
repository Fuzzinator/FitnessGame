using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

public class GarbageCollectorController : MonoBehaviour
{
    [SerializeField]
    private int _frameCount = 10;

    private CancellationToken _cancellationToken;
    
    // Start is called before the first frame update
    void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
        //BackgroundGarbageCollectorAsync().Forget();
    }

    public void SetGarbageCollectorState(bool enabled)
    {
        GarbageCollector.GCMode = enabled ? 
            GarbageCollector.Mode.Enabled :
            GarbageCollector.Mode.Disabled;
    }

    public void RunGarbageCollector()
    {
        RunGarbageCollectorAsync().Forget();
    }

    private async UniTaskVoid RunGarbageCollectorAsync()
    {
        for (var i = 0; i < _frameCount; i++)
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
            if (_cancellationToken.IsCancellationRequested)
            {
                return;
            }

            GarbageCollector.CollectIncremental(1000000 );
        }
    }

    private async UniTaskVoid BackgroundGarbageCollectorAsync()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            GarbageCollector.CollectIncremental(100000);
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken:_cancellationToken);
        }
    }
}