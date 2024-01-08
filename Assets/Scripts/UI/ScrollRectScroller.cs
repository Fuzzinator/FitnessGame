using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ScrollRectScroller : MonoBehaviour
{
    [SerializeField]
    private ScrollRect _scrollRect;

    [SerializeField]
    private float _scrollSpeed = 2f;

    private bool _scroll = false;

    private int _subscriberCount = 0;

    private const string LEFTJOYSTICKMOVING = "Left Joystick";
    private const string RIGHTJOYSTICKMOVING = "Right Joystick";

    public void ScrollY(float value)
    {
        if (_scroll)
        {
            return;
        }

        _scroll = true;
        AsyncScrollY(_scrollSpeed * value).Forget();
    }

    public void ScrollX(float value)
    {
        if (_scroll)
        {
            return;
        }

        _scroll = true;

        AsyncScrollX(_scrollSpeed * value).Forget();
    }

    public void StopScroll()
    {
        _scroll = false;
    }

    private void OnDestroy()
    {
        _scroll = false;
        TryUnsubscribeFromJoystick(true);
    }

    public void SubscribeToJoystick()
    {
        TrySubscribeToJoystick();
    }

    private void TrySubscribeToJoystick()
    {
        if (_subscriberCount == 0)
        {
            InputManager.Instance.MainInput[LEFTJOYSTICKMOVING].performed += JoystickScroll;
            InputManager.Instance.MainInput[RIGHTJOYSTICKMOVING].performed += JoystickScroll;
        }
        _subscriberCount++;
    }

    public void UnsubscribeFromJoystick()
    {
        TryUnsubscribeFromJoystick(false);
    }

    private void TryUnsubscribeFromJoystick(bool ignoreSubCount)
    {
        if (_subscriberCount > 0)
        {
            _subscriberCount--;
        }
        if (InputManager.Instance == null || (!ignoreSubCount && _subscriberCount > 0))
        {
            return;
        }
        InputManager.Instance.MainInput[LEFTJOYSTICKMOVING].performed -= JoystickScroll;
        InputManager.Instance.MainInput[RIGHTJOYSTICKMOVING].performed -= JoystickScroll;
    }

    private void JoystickScroll(InputAction.CallbackContext obj)
    {
        var value = obj.ReadValue<Vector2>();
        var initial = value;
        if (value != Vector2.zero && !_scroll)
        {
            _scroll = true;
            var rect = _scrollRect.content.rect;
            var rectWidth = rect.width;
            var rectHeight = rect.height;

            value.x /= rectWidth;
            value.y /= rectHeight;

            value *= -_scrollSpeed;

            var xPosition = Mathf.Clamp(_scrollRect.horizontalNormalizedPosition - value.x, 0, 1);
            _scrollRect.horizontalNormalizedPosition = xPosition;

            var yPosition = Mathf.Clamp(_scrollRect.verticalNormalizedPosition - value.y, 0, 1);
            _scrollRect.verticalNormalizedPosition = yPosition;

            DelayStopScroll().Forget();
        }
    }

    private async UniTaskVoid DelayStopScroll()
    {
        await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
        _scroll = false;
    }

    private async UniTask AsyncScrollY(float value)
    {
        var rectHeight = _scrollRect.content.rect.height;
        value /= rectHeight;

        while (_scroll)
        {
            await UniTask.DelayFrame(1);
            var position = Mathf.Clamp(_scrollRect.verticalNormalizedPosition - value, 0, 1);
            _scrollRect.verticalNormalizedPosition = position;
        }
    }

    private async UniTask AsyncScrollX(float value)
    {
        var rectWidth = _scrollRect.content.rect.width;
        value /= rectWidth;

        while (_scroll)
        {
            await UniTask.DelayFrame(1);
            var position = Mathf.Clamp(_scrollRect.horizontalNormalizedPosition - value, 0, 1);
            _scrollRect.horizontalNormalizedPosition = position;
        }
    }
}