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
    
    private TouchScreenKeyboard _keyboard;

    private CancellationTokenSource _cancellationSource;


    public async void StartEditTextField()
    {
        _keyboard = TouchScreenKeyboard.Open(_defaultText, TouchScreenKeyboardType.Default);
        _keyboard.characterLimit = 32;
        var source = new CancellationTokenSource();
        source = CancellationTokenSource.CreateLinkedTokenSource(source.Token, this.GetCancellationTokenOnDestroy());
        _cancellationSource = source;
        GameStateManager.Instance.gameStateChanged.AddListener(ListenForFocus);
        await UniTask.DelayFrame(1);
        await UpdateTextField();
    }

    private async UniTask UpdateTextField()
    {
        while (!_cancellationSource.IsCancellationRequested && TouchScreenKeyboard.visible)
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancellationSource.Token);
            if (_textField != null)
            {
                _textField.text = _keyboard.text;
            }
        }
    }

    private void ListenForFocus(GameState previousState, GameState newState)
    {
        if (previousState == GameState.Unfocused && previousState != newState)
        {
            EndEditTextField();
            _textField.text = _keyboard.text;
        }
    }

    public void EndEditTextField()
    {
        _cancellationSource.Cancel(false);
    }
}