using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR.Watch;
using YUR.Watch.Widgets;
using YUR.Core;

namespace YUR.UI.Displayers
{
    public class MainWidgetDisplayer : BaseDisplayer
    {

        public RectTransform mainDisplay;

        public RectTransform widgetDisplay;

        public RectTransform widgetRoot;

        private WidgetBase _currentWidget;

        private void Start()
        {
            WidgetManager.Instance.OnWidgetsInitialize += Setup;
            WidgetManager.Instance.OnDisconnectWidgets += DisableDisplay;
        }

        private void Setup(object obj = null)
        {
            string mainWidget = WidgetManager.Instance.widgetsSetup.face.widgetTypeID;
            WidgetType type = WidgetManager.Instance.GetEnumByString(mainWidget);
            SetWidget(WidgetManager.Instance.widgetsLibrary.GetWidget(type));

            if (YURInterface.Instance.HasLogin)
            {
                DisplayWidget(true);
            }
            else
            {
                DisplayWidget(false);
            }
        }

        private void DisableDisplay()
        {
            DisplayWidget(false); 
        }

        public void SetWidget(SOWidgetSetup widgetSetup)
        {
            if (_currentWidget)
            {
                Destroy(_currentWidget.gameObject); //TODO Change to not instantiate in realtime
            }
            _currentWidget = Instantiate(widgetSetup.widgetPrefab, widgetRoot);
        }

        public void DisplayWidget(bool showWidget)
        {
            if (widgetDisplay == null) return;
            widgetDisplay.gameObject.SetActive(showWidget);
            mainDisplay.gameObject.SetActive(!showWidget);
        }

        public override void Show(bool useAnimation, object obj = null)
        {
            if (debug)
                Debug.Log($"{name} is main screen, it can't be changed!");
        }

        public override void Hide(bool useAnimation, object obj = null)
        {
            if (debug)
                Debug.Log($"{name} is main screen, it can't be hide!");
        }
    }
}