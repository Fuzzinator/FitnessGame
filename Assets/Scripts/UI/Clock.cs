using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;

public class Clock : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    public bool use24HourTime = false;

    private async void Start()
    {
        await RunClock(this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
    }

    private async UniTask RunClock(CancellationToken token)
    {
        var delayLength = TimeSpan.FromMinutes(.25);
        var prevTime = DateTime.Now;
        await UniTask.Delay(delayLength, DelayType.Realtime, cancellationToken: token);
        while (enabled)
        {
            if (prevTime.Minute != DateTime.Now.Minute || prevTime.Second != DateTime.Now.Second)
            {
                var time = use24HourTime ? DateTime.Now.ToString("HH:mm") : DateTime.Now.ToShortTimeString();
                _text.SetText(time);
                prevTime = DateTime.Now;
            }

            await UniTask.Delay(delayLength, DelayType.Realtime, cancellationToken: token);
        }
    }
}