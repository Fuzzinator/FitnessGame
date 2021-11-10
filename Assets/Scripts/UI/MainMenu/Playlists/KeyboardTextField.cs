using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class KeyboardTextField : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _textField;

    [SerializeField]
    private string _defaultText;
    
    public void StartEditTextField()
    {
        KeyboardManager.Instance.ActivateKeyboard(_textField, _defaultText);
    }
}