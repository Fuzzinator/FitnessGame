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

    protected virtual async UniTask EditTextField()
    {
        var keyboard = KeyboardManager.Instance.ActivateKeyboard(_inputField, _defaultText);
        await UniTask.WaitWhile(() => keyboard.gameObject.activeInHierarchy);
        _editFieldCompleted?.Invoke(_inputField.text);
    }
}