using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitGame : MonoBehaviour
{
    private const string CONFIRMHEADER = "Quit?";
    private const string CONFIRMMESSAGE = "Are you sure you want to quit?";
    private const string CONFIRMBUTTON = "Yes";
    private const string CANCELBUTTON = "Cancel";
    
    public void RequestQuitConfirmation()
    {
        var display = new Notification.NotificationVisuals(CONFIRMMESSAGE,
            CONFIRMHEADER, CONFIRMBUTTON, CANCELBUTTON, disableUI: true);
        NotificationManager.RequestNotification(display, Quit);
    }
    
    
    private static void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
