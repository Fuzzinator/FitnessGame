using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR.Core;
using YUR.Watch;
using System.Linq;

namespace YUR.Watch.Widgets
{
    public class WidgetManager : Singleton<WidgetManager>
    {
        public SOWidgetLibrary widgetsLibrary;
        public List<WidgetType> widgets = new List<WidgetType>();

        public Action<YURWidgets> OnWidgetsInitialize;
        public Action OnDisconnectWidgets;
        public Action<YURProfile> OnProfileInitialize;

        public bool widgetsLoaded;

        public YURWidgets widgetsSetup => _widgetsSetup;
        private YURWidgets _widgetsSetup;

        public YURProfile yurProfile => _yurProfile;
        private YURProfile _yurProfile;

        protected void Start()
        {
            YURInterface.Instance.OnWidgetDataLoaded += OnWidgetsInitialized;
            YURInterface.Instance.OnLoadedSDKInitialized += OnProfileInitialized;
            YURInterface.Instance.OnLogout += OnDisconnect;
        }

        private void OnWidgetsInitialized(YURWidgets widgets)
        {

            bool faceWidgetChanged = false;

            if (_widgetsSetup != null)
            {
                if (!_widgetsSetup.face.widgetTypeID.Equals(widgets.face.widgetTypeID))
                {
                    faceWidgetChanged = true;
                }
            }

            _widgetsSetup = widgets;

            widgetsLoaded = true;

            List<WidgetType> lastWidgets = new List<WidgetType>();
            foreach (var item in this.widgets)
            {
                lastWidgets.Add(item);
            }

            UpdateWidgetsByString(widgets.sleeve1.widgetTypeID, widgets.sleeve2.widgetTypeID, widgets.sleeve3.widgetTypeID, widgets.sleeve4.widgetTypeID);


            if (lastWidgets.SequenceEqual(this.widgets) && !faceWidgetChanged)
            {
                return;
            }

            OnWidgetsInitialize?.Invoke(_widgetsSetup);
        }

        private void OnDisconnect()
        {
            widgets.Clear();
            widgetsLoaded = false;
            _widgetsSetup = null;

            OnDisconnectWidgets?.Invoke();
        }

        private void OnProfileInitialized(YURProfile profile)
        {
            _yurProfile = profile;

            OnProfileInitialize?.Invoke(_yurProfile);
        }

        public void UpdateWidgetsByString(params string[] ids)
        {
            widgets.Clear();
            foreach (string id in ids)
            {
                widgets.Add(GetEnumByString(id));
            }
        }

        public WidgetType GetEnumByString(string s)
        {
            switch (s)
            {
                case "YUR_TodayCalories":
                    return WidgetType.TodaysCalories;
                case "YUR_TodayCaloriesAndTime":
                    return WidgetType.TodaysCaloriesAndTime;
                case "YUR_Time":
                    return WidgetType.Clock;
                case "YUR_Level":
                    return WidgetType.YURLevel;
                case "YUR_HeartRate":
                    return WidgetType.YURHeartRate;
                case "YUR_Squats":
                    return WidgetType.TodaySquatCount;
                case "YUR_TodayTime":
                    return WidgetType.TimePlayedToday;
                case "YUR_WorkoutSquats":
                    return WidgetType.SquatsInWorkout;
                case "YUR_WorkoutTime":
                    return WidgetType.WorkoutTime;
                default:
                    return WidgetType.Clock;
            }
        }

    }
}