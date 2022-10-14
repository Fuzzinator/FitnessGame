using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestNotification : MonoBehaviour
{
    [SerializeField]
    private string _headerText;

    [SerializeField, TextArea]
    private string _bodyText;

    [SerializeField]
    private string _buttonText;

    [SerializeField]
    private bool _disableUI;
    
    public void Request()
    {
        var data = new Notification.NotificationVisuals(_bodyText, _headerText, _buttonText, disableUI: _disableUI);
        NotificationManager.RequestNotification(data);
    }
}
