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
        while (enabled)
        {
            var time = use24HourTime ? DateTime.Now.ToString("HH:mm") : DateTime.Now.ToShortTimeString();
            _text.SetText(time);
            await UniTask.Delay(TimeSpan.FromMinutes(.25), DelayType.Realtime,
                cancellationToken: token);
        }
    }
}