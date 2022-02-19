using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRKB;

public class LoadingDisplay : MonoBehaviour, IPoolable
{
    [SerializeField]
    private TextMeshProUGUI _textField;

    [SerializeField]
    private Image _loadingBar;

    private CancellationToken _cancellationToken;

    private bool _isCompleted = false;
    public PoolManager MyPoolManager { get; set; }
    public bool IsPooled { get; set; }

    
    private void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    public void SetUp(string message)
    {
        _isCompleted = false;
        _textField.text = message;
        _loadingBar.fillAmount = 0;
        gameObject.SetActive(true);
    }
    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        MyPoolManager.ReturnToPool(this);
    }

    public void UpdateLoadingBar(double value)
    {
        _loadingBar.fillAmount = (float)value;
        if (Math.Abs(value - 1) > .01f || _isCompleted)
        {
            return;
        }
        DisplayCompletedAsync().Forget();
    }

    private async UniTaskVoid DisplayCompletedAsync()
    {
        _isCompleted = true;
        await UniTask.SwitchToMainThread();
        _loadingBar.fillAmount = 1;
        _textField.text = $"{_textField.text} - COMPLETE";
        await UniTask.Delay(TimeSpan.FromSeconds(2.5));
        ReturnToPool();
    }
}
