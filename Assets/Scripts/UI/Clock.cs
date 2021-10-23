using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class Clock : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    public bool use24HourTime = false;
    
    private async void Start()
    {
        await RunClock();
    }

    private async UniTask RunClock()
    {
        while (enabled)
        {
            var time = use24HourTime ? DateTime.Now.ToString("HH:mm"):DateTime.Now.ToShortTimeString();
            _text.SetText(time);
            await UniTask.Delay(TimeSpan.FromMinutes(.25), DelayType.Realtime);
        }
    }
}
