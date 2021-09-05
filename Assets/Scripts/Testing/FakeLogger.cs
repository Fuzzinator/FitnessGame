using TMPro;
using UnityEngine;

public class FakeLogger : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textField;
    
    public static FakeLogger Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }
    }

    public static void Log(string text)
    {
        Instance._textField.SetText($"{Instance._textField.text}\n{text}");
    }
}
