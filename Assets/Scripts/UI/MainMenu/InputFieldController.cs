using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class InputFieldController : MonoBehaviour
{
    [SerializeField]
    protected TMP_InputField _inputField;

    [SerializeField]
    protected string _defaultText;

    [SerializeField]
    protected UnityEvent<string> _editFieldCompleted = new UnityEvent<string>();

    private const string UIInteraction = "";
    private const string ConfirmButton = "ConfirmText";

    public virtual void StartEditTextField()
    {
        EditTextField().Forget();
    }

    public virtual void ClearTextField()
    {
        _inputField.text = null;
        _editFieldCompleted?.Invoke(_inputField.text);
    }

    protected virtual async UniTask EditTextField()
    {
#if UNITY_STANDALONE_WIN
        var keyboard = KeyboardManager.Instance.ActivateKeyboard(_inputField, _defaultText);
        await UniTask.WaitWhile(() => keyboard.gameObject.activeInHierarchy);
        _editFieldCompleted?.Invoke(_inputField.text);
#elif UNITY_ANDROID
        FocusTracker.Instance.OnFocusChanged.AddListener(EditFieldCompleteFromFocus);
        await UniTask.DelayFrame(1);
#endif
    }

    private void EditFieldCompleteFromFocus(bool focusState)
    {
        if(!focusState)
        {
            return;
        }

        FocusTracker.Instance.OnFocusChanged.RemoveListener(EditFieldCompleteFromFocus);
        _editFieldCompleted?.Invoke(_inputField.text);
    }

    public void SetDefaultText(string value)
    {
        _defaultText = value;
    }
}