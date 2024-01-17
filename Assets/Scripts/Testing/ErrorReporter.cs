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

    private ES3Settings _settings;

    private const string ERRORLOG = "Log.dat";

    private const string ERRORTITLE = "An error has occurred.";

    private const string ErrorBodyRequestSendData =
        "An error has occured that may affect gameplay. Would you like to help improve Shadow BoXR by sending error logs?";

    private const string ERRORBODY =
        "An error has occured that may affect gameplay. If you run into issues, restarting the game may be required.";

    private const string HasRequestedHelp = "PlayerHasBeenAskedToSendErrorLogs";
    private const string PlayerAgreedToHelp = "PlayerHasAgreedToSendErrorLogs";

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
        _settings = new ES3Settings(ERRORLOG);
        if (ES3.DirectoryExists(_settings))
        {
            ES3.DeleteFile(_settings);
        }

        _callback = (text, stacktrace, logType) =>
        {
            LogMessage(text, stacktrace, logType);
            if (logType != LogType.Error)
            {
                return;
            }

            DisplayNotification(text, stacktrace);
            if (SettingsManager.GetSetting(PlayerAgreedToHelp, false))
            {

            }
        };
        Application.logMessageReceived += _callback;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= _callback;
    }

    public void LogMessage(string text, string stacktrace, LogType logType)
    {
        ES3.Save(DateTime.Now.ToString("yyyyMMddHHmmss"), $"{logType}: {text}\n{stacktrace}", _settings);
    }

    private void DisplayNotification(string text, string stacktrace)
    {
        var hasBeenPromptedToHelp = SettingsManager.GetSetting(HasRequestedHelp, false);
        if (hasBeenPromptedToHelp)
        {

            var visuals =
                new Notification.NotificationVisuals(ERRORBODY, ERRORTITLE, autoTimeOutTime: 5f, popUp: true);
            NotificationManager.RequestNotification(visuals, () => Log($"{text}\n{stacktrace}"));
        }
        else
        {
            var visuals = 
                new Notification.NotificationVisuals(ErrorBodyRequestSendData, ERRORTITLE, "Send Logs", "Opt Out", disableUI: true);
            NotificationManager.RequestNotification(visuals, () =>
            {
                var log = $"{text}\n{stacktrace}";
                Log(log);
                SendErrorLog(log);
            });
        }
    }


    private void SendErrorLog(string log)
    {
        AzureSqlManager.Instance.TrySendErrorReport(log);
    }

    public static void Log(string text)
    {
        Instance._debugCanvas.gameObject.SetActive(true);
        Instance._textField.SetTextZeroAlloc(text, true);
    }
}