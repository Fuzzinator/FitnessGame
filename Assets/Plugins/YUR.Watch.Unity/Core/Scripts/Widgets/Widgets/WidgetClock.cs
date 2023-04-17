using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using YUR.Core;

namespace YUR.Watch.Widgets
{
    public class WidgetClock : WidgetBase //TODO The time is from the application place. We will need the check if it is the same at the web app
    {
        private void Update()
        {
            SetTime();
        }

        private void SetTime()
        {
            string timeNow = $"{GetTime()}\n{GetTimePeriod()}";

            uiText.text = timeNow;
        }

        public static string GetTime()
        {
            DateTime time = DateTime.Now;

            string value = time.ToString("tt", CultureInfo.InvariantCulture);

            string timeNow = $"{time.Hour.ToString("00")}:{time.Minute.ToString("00")}";
            return timeNow;
        }

        public static string GetTimePeriod()
        {
            DateTime time = DateTime.Now;

            string value = time.ToString("tt", CultureInfo.InvariantCulture);

            return value;
        }

    }
}