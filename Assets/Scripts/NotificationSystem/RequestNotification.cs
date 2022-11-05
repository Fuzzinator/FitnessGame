using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class RequestNotification : MonoBehaviour
{
    [SerializeField]
    private string _headerText;

    [SerializeField, TextArea]
    private string _bodyText;

    [SerializeField, FormerlySerializedAs("_buttonText")]
    private string _button1Text;
    [SerializeField]
    private string _button2Text;
    [SerializeField]
    private string _button3Text;

    [SerializeField]
    private UnityEvent _button1Pressed;
    [SerializeField]
    private UnityEvent _button2Pressed;
    [SerializeField]
    private UnityEvent _button3Pressed;
    
    [SerializeField]
    private bool _disableUI;

    [SerializeField]
    private bool _requestOnEnable = false;

    private void OnEnable()
    {
        if (!_requestOnEnable)
        {
            return;
        }
        Request();
    }

    public void Request()
    {
        var data = new Notification.NotificationVisuals(
            _bodyText,
            _headerText, 
            _button1Text,
            _button2Text,
            _button3Text,
            _disableUI);
        NotificationManager.RequestNotification(
            data,
            () => _button1Pressed?.Invoke(), 
            () => _button2Pressed?.Invoke(),
            () => _button3Pressed?.Invoke());
    }
}
