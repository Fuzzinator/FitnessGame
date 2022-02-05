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
    private TextMeshProUGUI _hourText;

    [SerializeField]
    private TextMeshProUGUI _minuteText;

    [SerializeField]
    private TextMeshProUGUI _amPMDisplayText;

    public bool use24HourTime = false;

    private const string AM = "AM";
    private const string PM = "PM";
    
    private async void Start()
    {
        await RunClock(this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
    }

    private async UniTask RunClock(CancellationToken token)
    {
        var delayLength = TimeSpan.FromMinutes(.25);
        SetClock(out DateTime prevTime);
        await UniTask.Delay(delayLength, DelayType.Realtime, cancellationToken: token);
        while (enabled)
        {
            if (prevTime.Minute != DateTime.Now.Minute || prevTime.Second != DateTime.Now.Second)
            {
                SetClock(out prevTime);
            }

            await UniTask.Delay(delayLength, DelayType.Realtime, cancellationToken: token);
        }
    }

    private void SetClock(out DateTime prevTime)
    {
        _amPMDisplayText.SetText(use24HourTime?string.Empty:DateTime.Now.Hour<12?AM:PM);
        
        var hour = use24HourTime?DateTime.Now.Hour:DateTime.Now.Hour.To12HrFormat();
        _hourText.SetText(hour.GetCachedSecondsString());
        _minuteText.SetText(DateTime.Now.Minute.GetCachedSecondsString());
        
        prevTime = DateTime.Now;
    }
}