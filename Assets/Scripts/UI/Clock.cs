using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;

public class Clock : MonoBehaviour
{
    [FormerlySerializedAs("_hourText")] [SerializeField]
    private TextMeshProUGUI _timeText;

    public bool use24HourTime = false;

    private const string FORMAT = "{0}:{1}";
    private const string FORMAT00 = "{0}:{1}{2}";
    
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
        var hour = use24HourTime?DateTime.Now.Hour:DateTime.Now.Hour.To12HrFormat();
        var minute = DateTime.Now.Minute;
        using (var sb = ZString.CreateStringBuilder(true))
        {
            if (minute >= 10)
            {
                sb.AppendFormat(FORMAT, hour, minute);
            }
            else
            {
                
                sb.AppendFormat(FORMAT, hour, 0,minute);
            }

            if (!use24HourTime)
            {
                sb.Append(DateTime.Now.Hour < 12 ? AM : PM);
            }

            var buffer = sb.AsArraySegment();
            _timeText.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
        };
        
        prevTime = DateTime.Now;
    }
}