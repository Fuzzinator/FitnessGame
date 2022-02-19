using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    private int _index;
    private float[] _fps;

    private async void Start()
    {
        _index = 0;
        _fps = new float[5];
        await RunCounter(this.GetCancellationTokenOnDestroy());
    }

    private async UniTask RunCounter(CancellationToken token)
    {
        var delayLength = TimeSpan.FromSeconds(.05);
        while (enabled)
        {
            try
            {

                await UniTask.Delay(delayLength, cancellationToken: token);

                if (this == null || gameObject == null)
                {
                    return;
                }

                _fps[_index] = 1 / Time.unscaledDeltaTime;
                if (_index + 1 < _fps.Length)
                {
                    _index++;
                }
                else
                {
                    _index = 0;

                    var total = 0f;
                    foreach (var frame in _fps)
                    {
                        total += frame;
                    }

                    total /= _fps.Length;
                    using (var sb = ZString.CreateStringBuilder(true))
                    {
                        sb.Append((int)Mathf.Round(total));

                        var buffer = sb.AsArraySegment();
                        _text.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
                    }
                }
            }
            catch (Exception e) when (e is OperationCanceledException)
            {
                break;
            }
        }
    }
}