using System;
using TMPro;
using UnityEngine;

public class FakeLogger : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textField;
    
    public static FakeLogger Instance { get; private set; }

    private Application.LogCallback _callback;

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
                            //return;
                        }
                        Log(text);
                        Log(stacktrace);
                        //Log(stacktrace);
                    };
        Application.logMessageReceived += _callback;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= _callback;
    }

    public static void Log(string text)
    {
        Instance._textField.SetText($"{Instance._textField.text}\n{text}");
    }
}
