using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateNotifier : MonoBehaviour
{
    [SerializeField]
    private Notification.NotificationVisualInfo _notification;
    [SerializeField]
    private Toggle _updatesDisplayToggle;
    [SerializeField]
    private DisplayUpdates _updatesDisplay;

    private const int HelpPage = 5;
    private const int UpdatesPage = 4;

    public void NotifyOfUpdate()
    {
        _notification.message = _updatesDisplay.MostRecentUpdate.ShortDescription;
        NotificationManager.RequestNotification(_notification, ViewUpdateInfo);
    }

    private void ViewUpdateInfo()
    {
        MainMenuUIController.Instance.SetActivePage(HelpPage);
        _updatesDisplayToggle.isOn = true;
        //_menuController.SetActivePage(UpdatesPage);
    }
}
