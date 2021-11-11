using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
        await RunCounter();
    }

    private async UniTask RunCounter()
    {
        while (enabled)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(.1), cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();
            _fps[_index] = 1 / Time.deltaTime;

            if (gameObject == null)
            {
                return;
            }
            
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
                _text.SetText(Mathf.Round(total).ToString(CultureInfo.InvariantCulture));
            }

        }
    }
}