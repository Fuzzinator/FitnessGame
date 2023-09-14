using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class InputFieldController : MonoBehaviour
{
    [SerializeField]
    protected TMP_InputField _inputField;

    [SerializeField]
    protected string _defaultText;

    [SerializeField]
    protected UnityEvent<string> _editFieldCompleted = new UnityEvent<string>();

    public virtual void StartEditTextField()
    {
        EditTextField().Forget();
    }

    public virtual void ClearTextField()
    {
        _inputField.text = null;
    }
    
    protected virtual async UniTask EditTextField()
    {
#if UNITY_STANDALONE_WIN
        var keyboard = KeyboardManager.Instance.ActivateKeyboard(_inputField, _defaultText);
        await UniTask.WaitWhile(() => keyboard.gameObject.activeInHierarchy);
#elif UNITY_ANDROID
        await UniTask.WaitWhile(() => !FocusTracker.Instance.IsFocused);//keyboard.gameObject.activeInHierarchy);
#endif
        _editFieldCompleted?.Invoke(_inputField.text);
    }

    public void SetDefaultText(string value)
    {
        _defaultText = value;
    }
}