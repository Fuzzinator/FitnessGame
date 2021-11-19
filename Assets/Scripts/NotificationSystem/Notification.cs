using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class Notification : MonoBehaviour, IPoolable
{
    [SerializeField]
    private TextMeshProUGUI _message;

    [SerializeField]
    private TextMeshProUGUI _button1Txt;
    [SerializeField]
    private TextMeshProUGUI _button2Txt;
    [SerializeField]
    private TextMeshProUGUI _button3Txt;

    private Action _button1Pressed;
    private Action _button2Pressed;
    private Action _button3Pressed;

    private float _autoTimeOutTime;
    
    private PoolManager _myPoolManager;
    private bool _isPooled;

    private CancellationToken _cancellationToken;

    public PoolManager MyPoolManager
    {
        get => _myPoolManager;
        set => _myPoolManager = value;
    }

    public bool IsPooled
    {
        get => _isPooled;
        set => _isPooled = value;
    }

    private void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    public async void SetUpObject(NotificationVisuals visuals, Action button1Pressed = null, Action button2Pressed = null, Action button3Pressed = null)
    {
        _message.SetText(visuals.message);
        _button1Txt.SetText(visuals.button1Txt);
        _button2Txt.SetText(visuals.button2Txt);
        _button3Txt.SetText(visuals.button3Txt);
        _autoTimeOutTime = visuals.autoTimeOutTime;
        _button1Pressed = button1Pressed;
        _button2Pressed = button2Pressed;
        _button3Pressed = button3Pressed;

        gameObject.SetActive(true);

        if (!(_autoTimeOutTime > 0))
        {
            return;
        }
        
        await UniTask.Delay(TimeSpan.FromSeconds(_autoTimeOutTime), cancellationToken: _cancellationToken);
        ReturnToPool();
    }
    
    public void ReturnToPool()
    {
        _message.SetText(string.Empty);
        _button1Txt.SetText(string.Empty);
        _button2Txt.SetText(string.Empty);
        _button3Txt.SetText(string.Empty);
        _autoTimeOutTime = 0;

        _button1Pressed = null;
        _button2Pressed = null;
        _button3Pressed = null;
        
        gameObject.SetActive(false);
        
        MyPoolManager.ReturnToPool(this);
    }

    public void Button1Pressed()
    {
        _button1Pressed?.Invoke();
    }
    
    public void Button2Pressed()
    {
        _button2Pressed?.Invoke();
    }
    public void Button3Pressed()
    {
        _button3Pressed?.Invoke();
    }

    public struct NotificationVisuals
    {
        public string message;
        public string button1Txt;
        public string button2Txt;
        public string button3Txt;
        public float autoTimeOutTime;
    }
}
