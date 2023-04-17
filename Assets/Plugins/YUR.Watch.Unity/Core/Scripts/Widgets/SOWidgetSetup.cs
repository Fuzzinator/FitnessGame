using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YUR.Watch.Widgets
{
    [CreateAssetMenu(fileName = "SO_WidgetSetup", menuName = "Widgets/WidgetSetup")]
    public class SOWidgetSetup : ScriptableObject
    {
        public WidgetType widgetType;
        public WidgetBase widgetPrefab;
    }
}