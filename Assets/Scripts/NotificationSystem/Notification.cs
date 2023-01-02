using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Notification : MonoBehaviour, IPoolable
{
    [SerializeField] private TextMeshProUGUI _message;

    [SerializeField] private Button _button1;

    [SerializeField] private Button _button2;

    [SerializeField] private Button _button3;

    [SerializeField] private TextMeshProUGUI _button1Txt;

    [SerializeField] private TextMeshProUGUI _button2Txt;

    [SerializeField] private TextMeshProUGUI _button3Txt;

    [SerializeField] private GameObject _buttonsParent;

    private Action _button1Pressed;
    private Action _button2Pressed;
    private Action _button3Pressed;

    private float _autoTimeOutTime;
    private bool _disableUI;

    private PoolManager _myPoolManager;
    private bool _isPooled;

    private CancellationToken _cancellationToken;

    [SerializeField] private Canvas _canvas;

    private const string HEADERSTART = "<size=100><uppercase><b>";
    private const string HEADEREND = "</uppercase></size></b>\n";
    private const string MESSAGEFORMAT = "<size=100><uppercase><b>{0}</uppercase></size></b>\n{1}";
    private const float BASEHEIGHT = 720;
    private const float NOBUTTONHEIGHT = 530;

    public PoolManager MyPoolManager
    {
        get => _myPoolManager;
        set => _myPoolManager = value;
    }

    public bool IsPooled
    {
        get => _isPooled;
        set => _isPooled = value;
    }

    public void Initialize()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
        if (_canvas == null)
        {
            TryGetComponent(out _canvas);
        }
    }

    private void OnEnable()
    {
        if (string.IsNullOrWhiteSpace(_button1Txt.text) && string.IsNullOrWhiteSpace(_button1Txt.text) &&
            string.IsNullOrWhiteSpace(_button1Txt.text))
        {
            return;
        }

        var hasCanvas = _canvas != null;
        if (!hasCanvas)
        {
            hasCanvas = TryGetComponent(out _canvas);
        }

        if (!hasCanvas)
        {
            return;
        }

        _canvas.worldCamera = Head.Instance.HeadCamera;
        UIStateManager.Instance.RequestEnableInteraction(_canvas);
    }

    private void OnDisable()
    {
        UIStateManager.Instance.RequestDisableInteraction(_canvas);
    }

    public async void SetUpObject(NotificationVisuals visuals, Action button1Pressed = null,
        Action button2Pressed = null, Action button3Pressed = null)
    {
        string fullMessage;
        if (!string.IsNullOrWhiteSpace(visuals.header))
        {
            fullMessage = $"{HEADERSTART}{visuals.header}{HEADEREND}{visuals.message}";
        }
        else
        {
            fullMessage = visuals.message;
        }

        _message.SetText(fullMessage);
        var hasBttn1 = !string.IsNullOrWhiteSpace(visuals.button1Txt);
        if (hasBttn1)
        {
            _button1Txt.SetText(visuals.button1Txt);
            _button1.gameObject.SetActive(true);
        }

        var hasBttn2 = !string.IsNullOrWhiteSpace(visuals.button2Txt);
        if (hasBttn2)
        {
            _button2Txt.SetText(visuals.button2Txt);
            _button2.gameObject.SetActive(true);
        }

        var hasBttn3 = !string.IsNullOrWhiteSpace(visuals.button3Txt);
        if (hasBttn3)
        {
            _button3Txt.SetText(visuals.button3Txt);
            _button3.gameObject.SetActive(true);
        }

        _buttonsParent.SetActive(hasBttn1 || hasBttn2 || hasBttn3);

        if (transform is RectTransform rectTransform)
        {
            var targetHeight = visuals.height;
            if (targetHeight == 0f)
            {
                if (!hasBttn1 && !hasBttn2 && !hasBttn3)
                {
                    targetHeight = NOBUTTONHEIGHT;
                }
                else
                {
                    targetHeight = BASEHEIGHT;
                }
            }

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, targetHeight);
        }

        _disableUI = visuals.disableUI;
        _autoTimeOutTime = visuals.autoTimeOutTime;
        _button1Pressed = button1Pressed;
        _button2Pressed = button2Pressed;
        _button3Pressed = button3Pressed;

        gameObject.SetActive(true);

        if (_disableUI && MainMenuUIController.Instance)
        {
            MainMenuUIController.Instance.RequestDisableUI(this);
        }

        if (visuals.hideOnSceneChange)
        {
            SceneManager.activeSceneChanged += ReturnOnSceneChange;
        }

        if (_autoTimeOutTime <= 0)
        {
            return;
        }

        await UniTask.Delay(TimeSpan.FromSeconds(_autoTimeOutTime), cancellationToken: _cancellationToken)
            .SuppressCancellationThrow();
        ReturnToPool();
    }

    public async void SetUpObject(NotificationVisualInfo visuals, Action button1Pressed = null,
        Action button2Pressed = null, Action button3Pressed = null)
    {
        ArraySegment<char> fullMessage;
        if (!string.IsNullOrWhiteSpace(visuals.header))
        {
            using (var sb = ZString.CreateStringBuilder(true))
            {
                sb.AppendFormat(MESSAGEFORMAT, visuals.header, visuals.message);
                fullMessage = sb.AsArraySegment();
            }
        }
        else
        {
            using (var sb = ZString.CreateStringBuilder(true))
            {
                sb.Append(visuals.message);
                fullMessage = sb.AsArraySegment();
            }
        }

        _message.SetText(fullMessage.Array, fullMessage.Offset, fullMessage.Count);
        var hasBttn1 = !string.IsNullOrWhiteSpace(visuals.button1Txt);
        if (hasBttn1)
        {
            _button1Txt.SetText(visuals.button1Txt);
            _button1.gameObject.SetActive(true);
        }

        var hasBttn2 = !string.IsNullOrWhiteSpace(visuals.button2Txt);
        if (hasBttn2)
        {
            _button2Txt.SetText(visuals.button2Txt);
            _button2.gameObject.SetActive(true);
        }

        var hasBttn3 = !string.IsNullOrWhiteSpace(visuals.button3Txt);
        if (hasBttn3)
        {
            _button3Txt.SetText(visuals.button3Txt);
            _button3.gameObject.SetActive(true);
        }

        _buttonsParent.SetActive(hasBttn1 || hasBttn2 || hasBttn3);

        if (transform is RectTransform rectTransform)
        {
            if (!hasBttn1 && !hasBttn2 && !hasBttn3)
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, NOBUTTONHEIGHT);
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, BASEHEIGHT);
            }
        }

        _disableUI = visuals.disableUI;
        _autoTimeOutTime = visuals.autoTimeOutTime;
        _button1Pressed = button1Pressed;
        _button2Pressed = button2Pressed;
        _button3Pressed = button3Pressed;

        gameObject.SetActive(true);

        if (_disableUI && MainMenuUIController.Instance)
        {
            MainMenuUIController.Instance.RequestDisableUI(this);
        }

        if (visuals.hideOnSceneChange)
        {
            SceneManager.activeSceneChanged += ReturnOnSceneChange;
        }

        if (_autoTimeOutTime <= 0)
        {
            return;
        }

        await UniTask.Delay(TimeSpan.FromSeconds(_autoTimeOutTime), cancellationToken: _cancellationToken)
            .SuppressCancellationThrow();
        ReturnToPool();
    }

    private void ReturnOnSceneChange(Scene current, Scene newScene)
    {
        SceneManager.activeSceneChanged -= ReturnOnSceneChange;
        ReturnToPool();
    }

    public void ReturnToPool()
    {
        if (this == null || IsPooled)
        {
            return;
        }

        SceneManager.activeSceneChanged -= ReturnOnSceneChange;

        _message.SetText(string.Empty);
        _button1Txt.SetText(string.Empty);
        _button2Txt.SetText(string.Empty);
        _button3Txt.SetText(string.Empty);

        _button1.gameObject.SetActive(false);
        _button2.gameObject.SetActive(false);
        _button3.gameObject.SetActive(false);

        _autoTimeOutTime = 0;

        _button1Pressed = null;
        _button2Pressed = null;
        _button3Pressed = null;

        if (_disableUI)
        {
            MainMenuUIController.Instance?.RequestEnableUI(this);
        }

        _disableUI = true;

        gameObject.SetActive(false);

        MyPoolManager.ReturnToPool(this);
    }

    public void Button1Pressed()
    {
        _button1Pressed?.Invoke();
        ReturnToPool();
    }

    public void Button2Pressed()
    {
        _button2Pressed?.Invoke();
        ReturnToPool();
    }

    public void Button3Pressed()
    {
        _button3Pressed?.Invoke();
        ReturnToPool();
    }

    public struct NotificationVisuals
    {
        public string header;
        public string message;
        public string button1Txt;
        public string button2Txt;
        public string button3Txt;
        public bool disableUI;
        public float autoTimeOutTime;
        public bool popUp;
        public bool hideOnSceneChange;
        public float height;

        public NotificationVisuals(string message, string header = "", string button1Txt = "", string button2Txt = "",
            string button3Txt = "", bool disableUI = true, float autoTimeOutTime = 0f, bool popUp = false,
            bool hideOnSceneChange = false,
            float height = 0f)
        {
            this.header = header;
            this.message = message;
            this.button1Txt = button1Txt;
            this.button2Txt = button2Txt;
            this.button3Txt = button3Txt;
            this.disableUI = disableUI;
            this.autoTimeOutTime = autoTimeOutTime;
            this.popUp = popUp;
            this.hideOnSceneChange = hideOnSceneChange;
            this.height = height;
        }
    }

    public class NotificationVisualInfo
    {
        public string header;
        public string message;
        public string button1Txt = null;
        public string button2Txt = null;
        public string button3Txt = null;
        public bool disableUI = true;
        public float autoTimeOutTime = 0f;
        public bool popUp = false;
        public bool hideOnSceneChange = true;
        public float height = 0f;
    }
}