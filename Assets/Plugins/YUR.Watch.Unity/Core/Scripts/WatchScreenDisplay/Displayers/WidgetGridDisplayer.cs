using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR.Core;
using YUR.Watch.Widgets;

namespace YUR.UI.Displayers
{
    public class WidgetGridDisplayer : BaseDisplayer
    {
        public List<WidgetBase> currentWidgets;

        private bool initialized;

        private void Start()
        {
            WidgetManager.Instance.OnWidgetsInitialize += ReSetupWidgets;
        }

        private void ReSetupWidgets(YURWidgets widgets)
        {
            foreach (var item in currentWidgets)
            {
                Destroy(item.gameObject);
            }

            currentWidgets.Clear();
            initialized = false;
            SetupWidgets();
        }

        private void OnEnable()
        {
            SetupWidgets();
        }

        private void SetupWidgets()
        {
            if (!WidgetManager.Instance)
                return;

            if (!initialized && WidgetManager.Instance.widgetsLoaded)
            {
                StartWidgets();
                initialized = true;
            }
        }

        private void StartWidgets()
        {
            if (currentWidgets.Count > 0)
            {
                currentWidgets.ForEach(i => Destroy(i.gameObject));
            }

            currentWidgets = new List<WidgetBase>();

            foreach (var widget in WidgetManager.Instance.widgets)
            {
                var widgetObject = AddWidget(widget);
                if (widgetObject)
                    currentWidgets.Add(widgetObject);
            }
        }

        private WidgetBase AddWidget(WidgetType widgetType)
        {
            if (debug)
                Debug.Log("Try to add : " + widgetType);

            WidgetBase widget = GetWidgetByType(widgetType);

            if (widget)
                return Instantiate(widget, container.transform);
            else
            {
                if (debug)
                    Debug.Log($"<color='red'>At the library there isn't a widget of type</color> <color='green'>{widgetType.ToString()}</color>");
                return null;
            }
        }

        private WidgetBase GetWidgetByType(WidgetType widgetType)
        {
            return WidgetManager.Instance.widgetsLibrary.widgets.Find(i => i.widgetType == widgetType).widgetPrefab;
        }

    }
}