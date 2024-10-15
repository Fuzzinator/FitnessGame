using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelInitializer : MonoBehaviour
{
    [SerializeField]
    private IOrderedInitializeContainer[] _initializeOnAwake = Array.Empty<IOrderedInitializeContainer>();
    [SerializeField, FormerlySerializedAs("_initializees")]
    private IOrderedInitializeContainer[] _initializeOnStart = Array.Empty<IOrderedInitializeContainer>();

    private void Awake()
    {
        if(_initializeOnAwake == null || _initializeOnAwake.Length == 0)
        {
            return;
        }

        InitializeSet(_initializeOnAwake).Forget();
    }

    void Start()
    {
        if (_initializeOnStart == null || _initializeOnStart.Length == 0)
        {
            return;
        }

        InitializeSet(_initializeOnStart).Forget();
    }

    private async UniTaskVoid InitializeSet(IOrderedInitializeContainer[] set)
    {
        var timer = Stopwatch.StartNew();
        foreach (var toInitialize in set) 
        {
            toInitialize.Initialize();
            if(timer.ElapsedMilliseconds < 1)
            {
                continue;
            }
            await UniTask.NextFrame();
            timer.Restart();
        }
        timer.Stop();
    }
}
