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

public class Clock : MonoBehaviour, IOrderedInitialize
{
    public bool Initialized { get; private set; }

    [SerializeField]
    private TextMeshProUGUI _timeText;

    public bool use24HourTime = false;

    private const string FORMAT = "<mspace=.65em>{0}<mspace=.4em>:</mspace>{1:00} {2}</mspace>";
    
    private const string AM = "AM";
    private const string PM = "PM";
    
    public void Initialize()
    {
        if (Initialized)
        {
            return;
        }

        RunClock(this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow().Forget();
        Initialized = true;
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
            var ampm = use24HourTime ? string.Empty : DateTime.Now.Hour < 12 ? AM : PM;
            
            sb.AppendFormat(FORMAT, hour, minute, ampm);
            

            var buffer = sb.AsArraySegment();
            _timeText.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
        };
        
        prevTime = DateTime.Now;
    }
}