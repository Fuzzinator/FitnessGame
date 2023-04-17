using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YUR.Watch.Widgets
{
    [CreateAssetMenu(fileName = "SO_WidgetLibrary", menuName = "Widgets/WidgetLibrary")]
    public class SOWidgetLibrary : ScriptableObject
    {
        public List<SOWidgetSetup> widgets = new List<SOWidgetSetup>();

        public SOWidgetSetup GetWidget(WidgetType type)
        {
            return widgets.Find(w => w.widgetType == type);
        }
    }
}