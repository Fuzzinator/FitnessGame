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

    public virtual void StartEditTextField()
    {
        EditTextField().Forget();
    }

    public virtual void ClearTextField()
    {
        _textField.ClearText();
    }

    public void SetTargetText(TextMeshProUGUI textField)
    {
        _textField = textField;
    }

    protected virtual async UniTask EditTextField()
    {
#if UNITY_STANDALONE_WIN
        var keyboard = KeyboardManager.Instance.ActivateKeyboard(_textField, _textField.text);
        await UniTask.WaitWhile(() => keyboard.gameObject.activeInHierarchy);
#elif UNITY_ANDROID
        await UniTask.WaitWhile(() => !FocusTracker.Instance.IsFocused);//keyboard.gameObject.activeInHierarchy);
#endif
        _editFieldCompleted?.Invoke(_textField.text);
    }
}
