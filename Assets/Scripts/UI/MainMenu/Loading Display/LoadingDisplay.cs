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

    public float Progress { get; private set; }

    private CancellationToken _cancellationToken;

    private bool _isCompleted = false;
    public PoolManager MyPoolManager { get; set; }
    public bool IsPooled { get; set; }

    private BeatSageDownloadManager.Download _beatSageDownload;

    public void Initialize()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    public void SetUp(string message)
    {
        Progress = 0f;
        _isCompleted = false;
        _textField.text = message;
        _loadingBar.fillAmount = 0;
        gameObject.SetActive(true);
    }
    public void ReturnToPool()
    {
        Progress = 0;
        if (_beatSageDownload != null)
        {
            _beatSageDownload.ProgressUpdated.RemoveListener(HandleDownloadProgressChange);
            _beatSageDownload = null;
        }
        gameObject?.SetActive(false);
        MyPoolManager?.ReturnToPool(this);
    }

    public void UpdateLoadingBar(double value)
    {
        _loadingBar.fillAmount = (float)value;
        Progress = (float)value;
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
    public async UniTaskVoid DisplayFailedAsync(bool skipAwait = false)
    {
        _isCompleted = true;
        await UniTask.SwitchToMainThread();
        _loadingBar.fillAmount = 1;
        _textField.text = $"{_textField.text} - FAILED";
        if (!skipAwait)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2.5));
        }
        else
        {
            await UniTask.DelayFrame(1);
        }
        ReturnToPool();
    }

    public void SetUpBeatsageDownload(BeatSageDownloadManager.Download download)
    {
        _beatSageDownload = download;
        HandleDownloadProgressChange(download.Progress);
        _beatSageDownload.ProgressUpdated.AddListener(HandleDownloadProgressChange);
    }


    private void HandleDownloadProgressChange(double progress)
    {
        Progress = (float)progress;
        if (Progress < 0)
        {
            DisplayFailedAsync().Forget();
        }
        else
        {
            UpdateLoadingBar(progress);
        }
    }
}
