using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using YUR.Core;

namespace YUR.Watch.Widgets
{
    public class WidgetCaloriesAndTime : WidgetBase
    {

        public TextMeshProUGUI uiTimeText;

        protected void OnEnable()
        {
            Setup(YURInterface.Instance.cResults);
        }
        protected void Start()
        {
            YURInterface.Instance.OnCResultsLoaded += Setup;
        }
        protected void Setup(YUR_SDK.CResults data)
        {
            uiText.text = ((int)data.Calories).ToString();
        }

        private void Update()
        {
            SetTime();
        }

        private void SetTime()
        {
            string timeNow = $"{WidgetClock.GetTime()} {WidgetClock.GetTimePeriod()}";

            uiTimeText.text = timeNow;
        }

        protected override void UnSetup()
        {
             YURInterface.Instance.OnCResultsLoaded -= Setup;
        }
    }
}