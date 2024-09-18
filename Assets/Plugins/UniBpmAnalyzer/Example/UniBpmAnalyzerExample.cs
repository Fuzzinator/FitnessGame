/*
UniBpmAnalyzer
Copyright (c) 2016 WestHillApps (Hironari Nishioka)
This software is released under the MIT License.
http://opensource.org/licenses/mit-license.php
*/

using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class UniBpmAnalyzerExample : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] targetClip;

    private async void Start()
    {
        foreach (var clip in targetClip)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            var bpm = await /*UniBpmAnalyzer.TryAnalyzeBpm(clip);*/UniBpmAnalyzer.TryAnalyzeBpmWithJobs(clip);
            if (bpm < 0)
            {
                Debug.LogError("AudioClip is null.");
                return;
            }
            else
            {
                //Debug.Log(bpm.ToString());
            }
        }
    }
}
