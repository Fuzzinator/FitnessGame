using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class MainMenuUIController : BaseGameStateListener
{
    public static MainMenuUIController Instance { get; private set; }

    [SerializeField]
    private MenuPage[] _menuPages;

    [SerializeField]
    private int _targetDefaultPage = 0;

    private MenuPage _activeMenuPage;

    private List<MonoBehaviour> _requestSources = new List<MonoBehaviour>();

    public int MenuPageCount => _menuPages.Length;
    public int ActivePage { get; private set; }

    private bool _activePageSet = false;

    public static UnityEvent<int> OnMenuPageChange = new UnityEvent<int>();


    private void Awake()
    {
        if (Instance == null || Instance.gameObject == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        if (_activePageSet)
        {
            return;
        }

        SetActivePage(_targetDefaultPage);
    }

    protected new void OnDisable()
    {
        UIStateManager.Instance.RequestDisableInteraction(_activeMenuPage.TargetCanvas);
        CleanUpUI().Forget();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private async UniTaskVoid CleanUpUI()
    {
        var stopWatch = System.Diagnostics.Stopwatch.StartNew();
        for (var i = 0; i < _menuPages.Length; i++)
        {
            var page = _menuPages[i];
            Destroy(page.TargetCanvas.gameObject);
            if (stopWatch.ElapsedMilliseconds < 1)
            {
                continue;
            }

            await UniTask.NextFrame();
            stopWatch.Restart();
        }
        Destroy(gameObject);
    }

    public void SetActivePage(int targetPage)
    {
        SetActivePage(_menuPages[targetPage]);
        OnMenuPageChange?.Invoke(targetPage);
    }

    private void SetActivePage(MenuPage targetPage)
    {
        for (var i = 0; i < _menuPages.Length; i++)
        {
            var page = _menuPages[i];
            if (page != targetPage)
            {
                page.SetActive(0, false);
                UIStateManager.Instance.RequestDisableInteraction(page.TargetCanvas);
            }
            else
            {
                _activeMenuPage = page;
                _activePageSet = true;
                page.SetActive(1, true);
                ActivePage = i;
                UIStateManager.Instance.RequestEnableInteraction(_activeMenuPage.TargetCanvas);
            }
        }
    }

    protected override void GameStateListener(GameState oldState, GameState newState)
    {
        switch (newState)
        {
            case GameState.Paused:
            case GameState.Unfocused:
                RequestDisableUI(this);
                break;
            case GameState.Playing:
            case GameState.InMainMenu:
            case GameState.PreparingToPlay:
                RequestEnableUI(this);
                break;
        }
    }

    public void RequestDisableUI(MonoBehaviour behaviour)
    {
        if (_requestSources.Contains(behaviour))
        {
            return;
        }
        _requestSources.Add(behaviour);
        DisableUI();
    }

    public void RequestEnableUI(MonoBehaviour behaviour)
    {
        if (!_requestSources.Contains(behaviour))
        {
            return;
        }

        _requestSources.Remove(behaviour);
        if (_requestSources.Count == 0)
        {
            EnableUI();
        }
    }

    private void EnableUI()
    {
        if (gameObject == null)
        {
            return;
        }

        if (!_activeMenuPage.IsValid)
        {
            _activeMenuPage = _menuPages[0];
        }
        _activeMenuPage.SetActive(1, true);
    }

    private void DisableUI()
    {
        if (gameObject == null)
        {
            return;
        }

        if (!_activeMenuPage.IsValid)
        {
            _activeMenuPage = _menuPages[0];
        }
        _activeMenuPage.SetActive(.5f, false, true);
    }

    public void ReturnToHomeIfActive(Canvas canvas)
    {
        if (_activePageSet && _activeMenuPage.TargetCanvas == canvas)
        {
            SetActivePage(0);
        }
    }

    [Serializable]
    private struct MenuPage
    {
#if UNITY_EDITOR
        public string name;
#endif
        [SerializeField]
        private CanvasGroup _group;

        [SerializeField]
        private Canvas _canvas;

        [SerializeField]
        private GraphicRaycaster _graphicRaycaster;

        [SerializeField]
        private TrackedDeviceGraphicRaycaster _trackedDeviceRaycaster;

        public bool IsValid => _group != null && _canvas != null;
        public Canvas TargetCanvas => _canvas;
        public void SetActive(float alpha, bool enabled)
        {
            _group.SetGroupState(alpha, enabled);
            _graphicRaycaster.enabled = enabled;
            _trackedDeviceRaycaster.enabled = enabled;
            _canvas.enabled = enabled;
            _group.gameObject.SetActive(enabled);
        }

        public void SetActive(float alpha, bool enabled, bool canvasEnabled)
        {
            _group.SetGroupState(alpha, enabled);
            _graphicRaycaster.enabled = enabled;
            _trackedDeviceRaycaster.enabled = enabled;
            _canvas.enabled = canvasEnabled;
            _group.gameObject.SetActive(canvasEnabled);
        }


        public static bool operator ==(MenuPage page1, MenuPage page2)
        {
            return page1._canvas == page2._canvas &&
                   page1._group == page2._group &&
                   page1._graphicRaycaster == page2._graphicRaycaster &&
                   page1._trackedDeviceRaycaster == page2._trackedDeviceRaycaster;
        }

        public static bool operator !=(MenuPage page1, MenuPage page2)
        {
            return !(page1 == page2);
        }

        public override bool Equals(object obj)
        {
            if (obj is not MenuPage page)
            {
                return false;
            }
            return this == page;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_canvas, _group, _graphicRaycaster, _trackedDeviceRaycaster);
        }
    }
}
