using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR.Core;

namespace YUR.Watch.Widgets // TODO: Finish setup with AR
{
    public class WidgetTimePlayedToday : WidgetBase
    {
        protected void OnEnable()
        {
            Setup(YURInterface.Instance.cResults);
        }
        protected void Start()
        {
            YURInterface.Instance.OnCResultsLoaded += Setup;
        }

        private void Update()
        {
            Setup(null);
        }

        protected void Setup(YUR_SDK.CResults data)
        {
            TimeSpan time = new TimeSpan(0, 0, (int)Time.time);
            uiText.text = $"{time.Hours.ToString("00")}:{time.Minutes.ToString("00")}:{time.Seconds.ToString("00")}"; // TODO: Get right value with played time 
        }

        protected override void UnSetup()
        {
            YURInterface.Instance.OnCResultsLoaded -= Setup;
        }

    }
}