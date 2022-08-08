using System;
using TMPro;
using UnityEngine;

public class ErrorReporter : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textField;

    [SerializeField]
    private Canvas _debugCanvas;
    
    public static ErrorReporter Instance { get; private set; }

    private Application.LogCallback _callback;

    private const string ERRORTITLE = "An error has occurred.";

    private const string ERRORBODY =
        "A restart may be required. This is a Beta build and some of the kinks haven't been worked out yet.";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        _callback = (text, stacktrace, logType) =>
                    {
                        if (logType != LogType.Error)
                        {
                            //Log(text);
                            return;
                        }
                        DisplayNotification(text, stacktrace);
                    };
        Application.logMessageReceived += _callback;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= _callback;
    }

    private void DisplayNotification(string text, string stacktrace)
    {
        var visuals =
            new Notification.NotificationVisuals(ERRORBODY, ERRORTITLE, "Details", autoTimeOutTime: 5f, popUp: true);
        NotificationManager.RequestNotification(visuals, () => Log($"{text}\n{stacktrace}"));
    }
    
    public static void Log(string text)
    {
        Instance._debugCanvas.gameObject.SetActive(true);
        Instance._textField.SetText(text);
    }
}
