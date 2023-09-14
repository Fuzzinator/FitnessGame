using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EditUGUITextField : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _textField;

    [SerializeField]
    protected UnityEvent<string> _editFieldCompleted = new UnityEvent<string>();

    public virtual void StartEditTextField(string defaultText, string hiddensuffix)
    {
        EditTextField(defaultText, hiddensuffix).Forget();
    }

    public virtual void ClearTextField()
    {
        _textField.ClearText();
    }

    public void SetTargetText(TextMeshProUGUI textField)
    {
        _textField = textField;
    }

    protected virtual async UniTask EditTextField(string defaultText, string hiddenSuffix)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        var keyboard = KeyboardManager.Instance.ActivateKeyboard(_textField, defaultText);
        await UniTask.WaitWhile(() => KeyboardManager.Instance.status == KeyboardManager.Status.Visible);
        if(KeyboardManager.Instance.status == KeyboardManager.Status.Canceled)
        {
            return;
        }
        _textField.text = $"{_textField.text}{hiddenSuffix}";
#elif UNITY_ANDROID
        var keyboard = TouchScreenKeyboard.Open(defaultText);
        await UniTask.WaitWhile(() => keyboard.status == TouchScreenKeyboard.Status.Visible);//keyboard.gameObject.activeInHierarchy);
        if(keyboard.status == TouchScreenKeyboard.Status.Canceled)
        {
            return;
        }
        _textField.text = $"{keyboard.text}{hiddenSuffix}";
#endif
        _editFieldCompleted?.Invoke(_textField.text);
    }
}
