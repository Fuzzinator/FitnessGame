using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using YUR.Core;
using YUR.UI.Displayers;

namespace YUR.Watch
{
    public enum ScreenType
    {
        MainScreen,
        LoginScreen,
        WidgetScreen,
        PINScreen,
        LogoutScreen,
        ConfirmScreen,
        MessageScreen
    }
    public class WatchManager : Singleton<WatchManager>
    {
        public List<BaseDisplayer> displayers;

        public RectTransform watchRoot;

        public WatchDisplay watchDisplay;

        public ScreenType CurrentScreen => _currentScreen;
        public ScreenType LastScreen => _lastScreen;
        public ScreenType DefaultScreen => _defaultScreen;

        public bool debug = false;

        [SerializeField] private ScreenType _currentScreen = ScreenType.MainScreen;

        [SerializeField] private ScreenType _lastScreen = ScreenType.MainScreen;

        [SerializeField] private ScreenType _defaultScreen = ScreenType.WidgetScreen;

        private Coroutine _waitAnimationCO;
        private Coroutine _showingCO;

        protected override void Awake()
        {
            base.Awake();

            displayers = watchRoot.GetComponentsInChildren<BaseDisplayer>(true).ToList();
            LayoutRebuilder.ForceRebuildLayoutImmediate(watchRoot);
        }

        private void Start()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(watchRoot);
        }

        public void HideAll(ScreenType screenType, object obj = null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(watchRoot);

            Coroutine coroutine = null;

            if (watchDisplay)
                coroutine = watchDisplay.Hide(CurrentScreen == ScreenType.MainScreen);

            StartWaitAnimation(coroutine, () =>
             {
                 HideDisplayers();
                 LayoutRebuilder.ForceRebuildLayoutImmediate(watchRoot);
             });
        }

        private void HideDisplayers(object obj = null)
        {
            foreach (var manager in displayers)
            {
                manager.Hide(false, obj);

                if (manager.screenType != ScreenType.MainScreen)
                {
                    manager.gameObject.SetActive(false);
                }
            }
        }

        public void ShowByType(ScreenType screenType, object obj = null, bool overrideLast = false)
        {
            if (_showingCO != null)
            {
                StopCoroutine(_showingCO);
                _showingCO = null;
            }
            _showingCO = StartCoroutine(ShowByTypeCO(screenType, obj, overrideLast));
        }

        private IEnumerator ShowByTypeCO(ScreenType screenType, object obj = null, bool overrideLast = false)
        {
            SetScreen(screenType, overrideLast);


            HideAll(screenType);

            yield return _waitAnimationCO;

            displayers.FindAll(i => i.screenType == screenType).ForEach(x =>
            {
                x.gameObject.SetActive(true);
                x.Show(true, obj);
            });

            Coroutine coroutine = null;

            coroutine = watchDisplay.Show();

            StartWaitAnimation(coroutine, () =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(watchRoot);
            });

            _showingCO = null;
        }

        private void SetScreen(ScreenType screenType, bool overrideLast = false)
        {
            if (overrideLast)
            {
                if (screenType != _lastScreen)
                    _lastScreen = screenType;
            }
            else
            {
                if (screenType != _currentScreen)
                    _lastScreen = _currentScreen;
            }
            if (debug)
                Debug.Log($"<color='blue'> The last screen is {_lastScreen} and the current screen is {screenType}, the process {(overrideLast ? " " : "didn't")} overrided last</color>");

            _currentScreen = screenType;
        }

        public void SetCurrentAsDefaultScreen(bool overrideLast = false)
        {
            SetScreen(_defaultScreen, overrideLast);
        }

        public void SetLastAsDefaultScreen(bool overrideLast = false)
        {
            _lastScreen = _defaultScreen;
        }

        private void StartWaitAnimation(Coroutine animation, Action method)
        {
            if (_waitAnimationCO != null)
            {
                StopCoroutine(_waitAnimationCO);
            }

            _waitAnimationCO = StartCoroutine(WaitAnimation(animation, method));
        }

        private IEnumerator WaitAnimation(Coroutine animation, Action method)
        {
            yield return animation;

            method?.Invoke();

            _waitAnimationCO = null;
        }

    }

}